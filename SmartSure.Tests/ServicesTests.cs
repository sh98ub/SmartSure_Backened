using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Shared.Models;
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
using Microsoft.EntityFrameworkCore;
using AuthService.Infrasturcture.Data;
using PolicyService.Infrastructure.Data;
using ClaimService.Infrastructure.Data;
using AdminService.Infrastructure.Data;
using AdminService.Domain;

namespace SmartSure.Tests
{
    public class ServicesTests
    {
        private AuthDbContext GetAuthDbContext()
        {
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new AuthDbContext(options);
            context.Users.AddRange(new User[]
            {
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@smartsure.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123", workFactor: 12),
                    FullName = "System Administrator",
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new User
                {
                    Id = 2,
                    Username = "adjuster",
                    Email = "adjuster@smartsure.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Adjuster@123", workFactor: 12),
                    FullName = "Senior Claims Adjuster",
                    Role = UserRole.ClaimsAdjuster,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new User
                {
                    Id = 3,
                    Username = "john_doe",
                    Email = "john@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123", workFactor: 12),
                    FullName = "John Doe",
                    Role = UserRole.PolicyHolder,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            });
            context.SaveChanges();
            return context;
        }

        private PolicyDbContext GetPolicyDbContext()
        {
            var options = new DbContextOptionsBuilder<PolicyDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new PolicyDbContext(options);
            context.PolicyPlans.Add(new PolicyPlan 
            { 
                Id = 101, 
                Title = "Comprehensive Health Plan", 
                IsActive = true,
                Type = PolicyType.Health
            });
            context.SaveChanges();
            return context;
        }

        private ClaimDbContext GetClaimDbContext()
        {
            var options = new DbContextOptionsBuilder<ClaimDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ClaimDbContext(options);
        }

        private AdminDbContext GetAdminDbContext()
        {
            var options = new DbContextOptionsBuilder<AdminDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new AdminDbContext(options);
            context.AuditLogs.Add(new AuditLog
            {
                Id = 1,
                Timestamp = DateTime.UtcNow,
                Actor = "Admin",
                Action = "Create Plan",
                Details = "Created plan"
            });
            context.SaveChanges();
            return context;
        }

        [Fact]
        public async Task AuthService_UserRegistrationAndLogin_ShouldSucceed()
        {
            // Arrange
            var jwtGen = new JwtTokenGenerator();
            var userService = new UserService(GetAuthDbContext(), jwtGen);
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
        }

        [Fact]
        public async Task AuthService_RegistrationWithNameEmailPasswordOnly_ShouldSucceed()
        {
            // Arrange
            var jwtGen = new JwtTokenGenerator();
            var userService = new UserService(GetAuthDbContext(), jwtGen);
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
            Assert.False(string.IsNullOrEmpty(loginResult.Token));
        }

        [Fact]
        public async Task AuthService_SeededAdminLogin_ShouldReturnAdminRoleAndJwtToken()
        {
            // Arrange
            var jwtGen = new JwtTokenGenerator();
            var userService = new UserService(GetAuthDbContext(), jwtGen);
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
        }

        [Fact]
        public async Task AuthService_DuplicateUserRegistration_ShouldThrowUserAlreadyExistsException()
        {
            // Arrange
            var jwtGen = new JwtTokenGenerator();
            var userService = new UserService(GetAuthDbContext(), jwtGen);

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
            var userService = new UserService(GetAuthDbContext(), jwtGen);

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
            var policyService = new PolicyManagementService(GetPolicyDbContext());

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
            var policyService = new PolicyManagementService(GetPolicyDbContext());

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
            var claimService = new ClaimProcessingService(GetClaimDbContext());
            int userId = 101;
            int policyId = 1;

            var submitDto = new SubmitClaimDto
            {
                UserPolicyId = policyId,
                ClaimAmount = 1500.00m,
                Description = "Accidental windshield damage"
            };

            // Act - Submit
            var claim = await claimService.SubmitClaimAsync(submitDto, userId);

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
        public async Task ClaimService_GetUnapprovedClaims_ShouldReturnOnlySubmittedOrUnderReview()
        {
            // Arrange
            var claimService = new ClaimProcessingService(GetClaimDbContext());
            
            // Act
            var unapprovedClaims = await claimService.GetUnapprovedClaimsAsync();
            
            // Assert
            Assert.NotNull(unapprovedClaims);
            foreach (var c in unapprovedClaims)
            {
                Assert.True(c.Status == "Submitted" || c.Status == "UnderReview");
            }
        }

        [Fact]
        public async Task ClaimService_InvalidClaimAmount_ShouldThrowInvalidClaimAmountException()
        {
            // Arrange
            var claimService = new ClaimProcessingService(GetClaimDbContext());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidClaimAmountException>(async () =>
            {
                await claimService.SubmitClaimAsync(new SubmitClaimDto
                {
                    UserPolicyId = 1,
                    ClaimAmount = -500.00m, // Invalid negative amount
                    Description = "Invalid claim amount test"
                }, 1);
            });
        }

        [Fact]
        public async Task AdminCreatesPolicy_UserBuysPolicy_UserClaims_AdminApproves_EndToEndWorkflow()
        {
            // 1. Setup Services
            var policyService = new PolicyManagementService(GetPolicyDbContext());
            var claimService = new ClaimProcessingService(GetClaimDbContext());
            var adminService = new AdminDashboardService(GetAdminDbContext(), GetMockHttpClient(), null!, null!);

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
                UserPolicyId = boughtPolicy.Id,
                ClaimAmount = 750.00m,
                Description = "Root canal and porcelain crown treatment."
            };
            var submittedClaim = await claimService.SubmitClaimAsync(claimDto, userId);
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
            var adminService = new AdminDashboardService(GetAdminDbContext(), GetMockHttpClient(), null!, null!);

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
            var adminService = new AdminDashboardService(GetAdminDbContext(), GetMockHttpClient(), null!, null!);

            // Act - Fetch All Users
            var users = await adminService.GetUserOverviewListAsync();
            var userList = users.ToList();

            // Assert
            Assert.NotEmpty(userList);
            var john = userList.FirstOrDefault(u => u.Username == "john_doe");
            Assert.NotNull(john);

            // Act - Update Status
            var updatedUser = await adminService.UpdateUserStatusAsync(john.Id, new AdminUpdateUserStatusDto
            {
                IsActive = true
            });

            // Assert
            Assert.NotNull(updatedUser);
        }

        private HttpClient GetMockHttpClient()
        {
            var mockUsers = new List<AdminUserOverviewDto>
            {
                new AdminUserOverviewDto { Id = 1, Username = "admin", Email = "admin@smartsure.com", FullName = "System Administrator", Role = "Admin", CreatedAt = DateTime.UtcNow, IsActive = true },
                new AdminUserOverviewDto { Id = 2, Username = "adjuster", Email = "adjuster@smartsure.com", FullName = "Senior Claims Adjuster", Role = "ClaimsAdjuster", CreatedAt = DateTime.UtcNow, IsActive = true },
                new AdminUserOverviewDto { Id = 3, Username = "john_doe", Email = "john@example.com", FullName = "John Doe", Role = "PolicyHolder", CreatedAt = DateTime.UtcNow, IsActive = true }
            };

            var handler = new MockHttpMessageHandler(async (request) =>
            {
                var uri = request.RequestUri?.ToString() ?? "";
                
                if (uri.Contains("api/auth/admin/users") && request.Method == HttpMethod.Get)
                {
                    var segments = request.RequestUri?.Segments;
                    if (segments != null && segments.Length > 0)
                    {
                        var lastSegment = segments[^1].TrimEnd('/');
                        if (int.TryParse(lastSegment, out int userId))
                        {
                            var user = mockUsers.FirstOrDefault(u => u.Id == userId);
                            var apiResponse = ApiResponse<AdminUserOverviewDto>.SuccessResponse(user!, "User retrieved");
                            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                            {
                                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(apiResponse, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase }), System.Text.Encoding.UTF8, "application/json")
                            };
                        }
                    }

                    var listResponse = ApiResponse<List<AdminUserOverviewDto>>.SuccessResponse(mockUsers, "Users retrieved");
                    return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(listResponse, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase }), System.Text.Encoding.UTF8, "application/json")
                    };
                }
                else if (uri.Contains("status") && request.Method == HttpMethod.Put)
                {
                    var bodyStr = await request.Content!.ReadAsStringAsync();
                    var updateDto = System.Text.Json.JsonSerializer.Deserialize<AdminUpdateUserStatusDto>(bodyStr, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
                    
                    var segments = request.RequestUri?.Segments;
                    var lastSegment = segments?[^2].TrimEnd('/') ?? "";
                    if (int.TryParse(lastSegment, out int userId))
                    {
                        var user = mockUsers.FirstOrDefault(u => u.Id == userId);
                        if (user != null)
                        {
                            user.IsActive = updateDto!.IsActive;
                        }
                        var apiResponse = ApiResponse<AdminUserOverviewDto>.SuccessResponse(user!, "User updated");
                        return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                        {
                            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(apiResponse, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase }), System.Text.Encoding.UTF8, "application/json")
                        };
                    }
                }

                return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            });

            return new HttpClient(handler);
        }
    }

    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;

        public MockHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            return _handler(request);
        }
    }
}
