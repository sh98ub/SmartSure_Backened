using System;
using System.Linq;
using System.Threading.Tasks;
using AdminService.Application.DTOs;
using AdminService.Infrastructure.Services;
using AuthService.API.Services;
using AuthService.Application.DTOs;
using AuthService.Domain;
using AuthService.Domain.Exceptions;
using AuthService.Infrasturcture.Services;
using ClaimService.Application.DTOs;
using ClaimService.Domain;
using ClaimService.Domain.Exceptions;
using ClaimService.Infrastructure.Services;
using PolicyService.Application.DTOs;
using PolicyService.Domain;
using PolicyService.Domain.Exceptions;
using PolicyService.Infrastructure.Services;
using Xunit;

namespace SmartSure.Tests
{
    public class ServicesTests
    {
        [Fact]
        public async Task AuthService_UserRegistrationAndLogin_ShouldSucceed()
        {
            // Arrange
            var jwtGen = new JwtTokenGenerator();
            var userService = new UserService(jwtGen);
            var registerDto = new RegisterDto
            {
                Email = "testuser@example.com",
                Password = "Password@123",
                Name = "Test User"
            };

            // Act
            var regResult = await userService.RegisterAsync(registerDto);

            // Assert
            Assert.NotNull(regResult);
            Assert.Equal(registerDto.Name, regResult.User.FullName);
            Assert.Equal(registerDto.Email, regResult.User.Email);
            Assert.True(regResult.User.Id > 0);

            // Act - Login
            var loginDto = new LoginDto
            {
                Username = registerDto.Email,
                Password = registerDto.Password
            };
            var loginResult = await userService.LoginAsync(loginDto);

            // Assert
            Assert.NotNull(loginResult);
            Assert.False(string.IsNullOrEmpty(loginResult.Token));
            Assert.Equal(registerDto.Email, loginResult.User.Username);
        }

        [Fact]
        public async Task AuthService_RegistrationWithNameEmailPasswordOnly_ShouldSucceed()
        {
            // Arrange
            var jwtGen = new JwtTokenGenerator();
            var userService = new UserService(jwtGen);
            var email = "simple_" + Guid.NewGuid().ToString("N").Substring(0, 6) + "@example.com";
            var registerDto = new RegisterDto
            {
                Name = "Jane Doe",
                Email = email,
                Password = "Password@123"
            };

            // Act
            var regResult = await userService.RegisterAsync(registerDto);

            // Assert
            Assert.NotNull(regResult);
            Assert.Equal("Jane Doe", regResult.User.FullName);
            Assert.Equal(email, regResult.User.Email);
            Assert.Equal(email, regResult.User.Username);

            // Act - Login using Email as username
            var loginResult = await userService.LoginAsync(new LoginDto
            {
                Username = email,
                Password = "Password@123"
            });

            // Assert
            Assert.NotNull(loginResult);
            Assert.Equal("Jane Doe", loginResult.User.FullName);
        }

        [Fact]
        public async Task AuthService_SeededAdminLogin_ShouldReturnAdminRoleAndJwtToken()
        {
            // Arrange
            var jwtGen = new JwtTokenGenerator();
            var userService = new UserService(jwtGen);
            var adminLoginDto = new LoginDto
            {
                Username = "admin",
                Password = "Admin@123"
            };

            // Act
            var loginResult = await userService.LoginAsync(adminLoginDto);

            // Assert
            Assert.NotNull(loginResult);
            Assert.False(string.IsNullOrEmpty(loginResult.Token));
            Assert.Equal("admin", loginResult.User.Username);
            Assert.Equal("Admin", loginResult.User.Role);
        }

        [Fact]
        public async Task AuthService_DuplicateUserRegistration_ShouldThrowUserAlreadyExistsException()
        {
            // Arrange
            var jwtGen = new JwtTokenGenerator();
            var userService = new UserService(jwtGen);

            // Act & Assert
            await Assert.ThrowsAsync<UserAlreadyExistsException>(async () =>
            {
                await userService.RegisterAsync(new RegisterDto
                {
                    Email = "admin@smartsure.com", // Seeded Admin Email
                    Name = "Duplicate User",
                    Password = "Pass@123"
                });
            });
        }

        [Fact]
        public async Task AuthService_InvalidPassword_ShouldThrowInvalidCredentialsException()
        {
            // Arrange
            var jwtGen = new JwtTokenGenerator();
            var userService = new UserService(jwtGen);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidCredentialsException>(async () =>
            {
                await userService.LoginAsync(new LoginDto
                {
                    Username = "admin",
                    Password = "WrongPassword!123"
                });
            });
        }

        [Fact]
        public async Task PolicyService_GetPlansAndSubscribe_ShouldCreateActiveUserPolicy()
        {
            // Arrange
            var policyService = new PolicyManagementService();

            // Act
            var plans = await policyService.GetPolicyPlansAsync();
            var planList = plans.ToList();

            Assert.NotEmpty(planList);

            var planToSubscribe = planList.First();
            int userId = 101;

            var subscribeDto = new SubscribePolicyDto
            {
                UserId = userId,
                PolicyPlanId = planToSubscribe.Id
            };

            var userPolicy = await policyService.SubscribePolicyAsync(subscribeDto);

            // Assert
            Assert.NotNull(userPolicy);
            Assert.Equal(userId, userPolicy.UserId);
            Assert.True(userPolicy.Id > 0);
            Assert.StartsWith("POL-", userPolicy.PolicyNumber);
            Assert.Equal("Active", userPolicy.Status);

            var userPolicies = await policyService.GetUserPoliciesAsync(userId);
            Assert.NotEmpty(userPolicies);
        }

        [Fact]
        public async Task PolicyService_SubscribeInvalidPlan_ShouldThrowPolicyPlanNotFoundException()
        {
            // Arrange
            var policyService = new PolicyManagementService();

            // Act & Assert
            await Assert.ThrowsAsync<PolicyPlanNotFoundException>(async () =>
            {
                await policyService.SubscribePolicyAsync(new SubscribePolicyDto
                {
                    UserId = 1,
                    PolicyPlanId = 99999 // Invalid Plan ID
                });
            });
        }

        [Fact]
        public async Task ClaimService_SubmitAndReviewClaim_ShouldUpdateStatusAndPayout()
        {
            // Arrange
            var claimService = new ClaimProcessingService();
            int userId = 101;
            int policyId = 1;

            var submitDto = new SubmitClaimDto
            {
                UserId = userId,
                UserPolicyId = policyId,
                IncidentDate = DateTime.UtcNow.AddDays(-5),
                ClaimAmount = 1500.00m,
                Description = "Accidental windshield damage",
                SupportingDocumentUrl = "https://example.com/docs/windshield.pdf"
            };

            // Act - Submit
            var claim = await claimService.SubmitClaimAsync(submitDto);

            // Assert
            Assert.NotNull(claim);
            Assert.True(claim.Id > 0);
            Assert.StartsWith("CLM-", claim.ClaimNumber);
            Assert.Equal("Submitted", claim.Status);

            // Act - Review
            var reviewDto = new ReviewClaimDto
            {
                ClaimId = claim.Id,
                Status = ClaimStatus.Approved,
                ApprovedPayoutAmount = 1400.00m,
                Remarks = "Approved after coverage deductible calculation."
            };

            var reviewedClaim = await claimService.ReviewClaimAsync(reviewDto);

            // Assert
            Assert.NotNull(reviewedClaim);
            Assert.Equal("Approved", reviewedClaim.Status);
            Assert.Equal(1400.00m, reviewedClaim.ApprovedPayoutAmount);
        }

        [Fact]
        public async Task ClaimService_InvalidClaimAmount_ShouldThrowInvalidClaimAmountException()
        {
            // Arrange
            var claimService = new ClaimProcessingService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidClaimAmountException>(async () =>
            {
                await claimService.SubmitClaimAsync(new SubmitClaimDto
                {
                    UserId = 1,
                    UserPolicyId = 1,
                    ClaimAmount = -500.00m, // Invalid negative amount
                    Description = "Invalid claim amount test"
                });
            });
        }

        [Fact]
        public async Task AdminCreatesPolicy_UserBuysPolicy_UserClaims_AdminApproves_EndToEndWorkflow()
        {
            // 1. Setup Services
            var policyService = new PolicyManagementService();
            var claimService = new ClaimProcessingService();
            var adminService = new AdminDashboardService();

            // 2. Admin creates a new Policy Plan
            var newPlanDto = new CreatePolicyPlanDto
            {
                Title = "Dental & Optical Premium Shield",
                Description = "Full coverage for dental procedures and optical exams.",
                Type = PolicyType.Health,
                BasePremium = 145.00m,
                CoverageLimit = 15000.00m,
                DurationMonths = 12
            };
            var createdPlan = await policyService.CreatePolicyPlanAsync(newPlanDto);
            Assert.NotNull(createdPlan);
            Assert.True(createdPlan.Id > 0);

            // Log Admin activity
            await adminService.LogActivityAsync(new CreateAuditLogDto
            {
                Actor = "Admin",
                Action = "Create Policy Plan",
                Details = $"Created policy plan {createdPlan.Title}"
            });

            // 3. User buys the newly created policy
            int userId = 501;
            var subscribeDto = new SubscribePolicyDto
            {
                UserId = userId,
                PolicyPlanId = createdPlan.Id
            };
            var boughtPolicy = await policyService.SubscribePolicyAsync(subscribeDto);
            Assert.NotNull(boughtPolicy);
            Assert.Equal("Active", boughtPolicy.Status);

            // 4. User files a claim against the policy
            var claimDto = new SubmitClaimDto
            {
                UserId = userId,
                UserPolicyId = boughtPolicy.Id,
                IncidentDate = DateTime.UtcNow.AddDays(-2),
                ClaimAmount = 750.00m,
                Description = "Root canal and porcelain crown treatment."
            };
            var submittedClaim = await claimService.SubmitClaimAsync(claimDto);
            Assert.NotNull(submittedClaim);
            Assert.Equal("Submitted", submittedClaim.Status);

            // 5. Admin approves the claim
            var reviewDto = new ReviewClaimDto
            {
                ClaimId = submittedClaim.Id,
                Status = ClaimStatus.Approved,
                ApprovedPayoutAmount = 750.00m,
                Remarks = "Full payout approved by Admin"
            };
            var approvedClaim = await claimService.ReviewClaimAsync(reviewDto);
            Assert.NotNull(approvedClaim);
            Assert.Equal("Approved", approvedClaim.Status);
            Assert.Equal(750.00m, approvedClaim.ApprovedPayoutAmount);
        }

        [Fact]
        public async Task AdminService_GetMetricsAndAuditLogs_ShouldReturnValidData()
        {
            // Arrange
            var adminService = new AdminDashboardService();

            // Act
            var metrics = await adminService.GetDashboardMetricsAsync();
            var logs = await adminService.GetAuditLogsAsync();

            // Assert
            Assert.NotNull(metrics);
            Assert.True(metrics.TotalUsers > 0);
            Assert.True(metrics.ActivePolicies > 0);
            Assert.NotNull(logs);
            Assert.NotEmpty(logs);
        }

        [Fact]
        public async Task AdminService_UserManagement_ShouldReturnFullUserDetailsAndUpdateStatus()
        {
            // Arrange
            var adminService = new AdminDashboardService();

            // Act - Fetch All Users
            var users = await adminService.GetUserOverviewListAsync();
            var userList = users.ToList();

            // Assert
            Assert.NotEmpty(userList);
            var john = userList.FirstOrDefault(u => u.Username == "john_doe");
            Assert.NotNull(john);
            Assert.False(string.IsNullOrEmpty(john.PhoneNumber));
            Assert.False(string.IsNullOrEmpty(john.Address));
            Assert.Equal("Verified", john.KycStatus);

            // Act - Update Status & KYC
            var updatedUser = await adminService.UpdateUserStatusAsync(john.Id, new AdminUpdateUserStatusDto
            {
                IsActive = true,
                KycStatus = "Verified-Premium"
            });

            // Assert
            Assert.NotNull(updatedUser);
            Assert.Equal("Verified-Premium", updatedUser.KycStatus);
        }
    }
}
