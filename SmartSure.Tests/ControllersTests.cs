using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthService.API.Controllers;
using AuthService.API.Services;
using AuthService.Application.DTOs;
using AuthService.Infrasturcture.Services;
using ClaimService.API.Controllers;
using ClaimService.Application.DTOs;
using ClaimService.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PolicyService.APi.Controllers;
using PolicyService.Application.DTOs;
using PolicyService.Infrastructure.Services;
using Xunit;
using Shared.Models;
using Microsoft.EntityFrameworkCore;
using ClaimService.Infrastructure.Data;
using PolicyService.Infrastructure.Data;
using PolicyService.Domain;

namespace SmartSure.Tests
{
    public class ControllersTests
    {
        private ClaimDbContext GetClaimDbContext()
        {
            var options = new DbContextOptionsBuilder<ClaimDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ClaimDbContext(options);
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

        [Fact]
        public async Task UserClaimsController_GetClaimById_OwnClaim_ShouldReturnOk()
        {
            // Arrange
            var claimService = new ClaimProcessingService(GetClaimDbContext());
            var controller = new UserClaimsController(claimService);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "42")
                    }, "TestAuth"))
                }
            };

            var submitted = await claimService.SubmitClaimAsync(new SubmitClaimDto
            {
                UserPolicyId = 1,
                ClaimAmount = 500m,
                Description = "User 42 claim"
            }, 42);

            // Act
            var result = await controller.GetMyClaimById(submitted.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<ClaimDto>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal(42, apiResponse.Data.UserId);
        }

        [Fact]
        public async Task UserClaimsController_GetClaimById_OtherUserClaim_ShouldReturnForbidden()
        {
            // Arrange
            var claimService = new ClaimProcessingService(GetClaimDbContext());
            var controller = new UserClaimsController(claimService);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "999")
                    }, "TestAuth"))
                }
            };

            var submitted = await claimService.SubmitClaimAsync(new SubmitClaimDto
            {
                UserPolicyId = 1,
                ClaimAmount = 750m,
                Description = "User 100 claim"
            }, 100);

            // Act - User 999 attempting to view User 100's claim
            var result = await controller.GetMyClaimById(submitted.Id);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task AdminClaimsController_GetAllClaims_ShouldReturnClaimsList()
        {
            // Arrange
            var claimService = new ClaimProcessingService(GetClaimDbContext());
            var controller = new AdminClaimsController(claimService);

            // Act
            var result = await controller.GetAllClaims();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<object>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.NotNull(apiResponse.Data);
        }

        [Fact]
        public async Task UserPoliciesController_GetPolicyById_OtherUserPolicy_ShouldReturnForbidden()
        {
            // Arrange
            var policyService = new PolicyManagementService(GetPolicyDbContext());
            var controller = new UserPoliciesController(policyService);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "300")
                    }, "TestAuth"))
                }
            };

            var subscribed = await policyService.SubscribePolicyAsync(new SubscribePolicyDto
            {
                UserId = 200,
                PolicyPlanId = 101
            });

            // Act - User 300 attempting to view User 200's policy
            var result = await controller.GetMyPolicyById(subscribed.Id);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task UserPoliciesController_CancelPolicy_OtherUserPolicy_ShouldReturnForbidden()
        {
            // Arrange
            var policyService = new PolicyManagementService(GetPolicyDbContext());
            var controller = new UserPoliciesController(policyService);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "300")
                    }, "TestAuth"))
                }
            };

            var subscribed = await policyService.SubscribePolicyAsync(new SubscribePolicyDto
            {
                UserId = 200,
                PolicyPlanId = 101
            });

            // Act - User 300 attempting to cancel User 200's policy
            var result = await controller.CancelMyPolicy(subscribed.Id);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task AdminPoliciesController_CreatePlan_ShouldReturnCreatedPlan()
        {
            // Arrange
            var policyService = new PolicyManagementService(GetPolicyDbContext());
            var controller = new AdminPoliciesController(policyService);

            var dto = new CreatePolicyPlanDto
            {
                Title = "Super Admin Shield",
                Description = "Special plan",
                Type = PolicyService.Domain.PolicyType.Health,
                BasePremium = 5000m,
                CoverageLimit = 1000000m,
                DurationMonths = 24
            };

            // Act
            var result = await controller.CreatePolicyPlan(dto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, statusCodeResult.StatusCode);
            var apiResponse = Assert.IsType<ApiResponse<PolicyPlanDto>>(statusCodeResult.Value);
            Assert.Equal("Super Admin Shield", apiResponse.Data.Title);
        }

        [Fact]
        public void AuthController_GetAllUsersAndGetUserById_HaveAdminAuthorizeAttribute()
        {
            // Arrange
            var controllerType = typeof(AuthController);

            var getAllUsersMethod = controllerType.GetMethod(nameof(AuthController.GetAllUsers));
            var getUserByIdMethod = controllerType.GetMethod(nameof(AuthController.GetUserById));
            var updateStatusMethod = controllerType.GetMethod(nameof(AuthController.UpdateUserStatus));

            // Act & Assert
            Assert.NotNull(getAllUsersMethod);
            var getAllAttr = getAllUsersMethod.GetCustomAttribute<AuthorizeAttribute>();
            Assert.NotNull(getAllAttr);
            Assert.Equal("Admin", getAllAttr.Roles);

            Assert.NotNull(getUserByIdMethod);
            var getByIdAttr = getUserByIdMethod.GetCustomAttribute<AuthorizeAttribute>();
            Assert.NotNull(getByIdAttr);
            Assert.Equal("Admin", getByIdAttr.Roles);

            Assert.NotNull(updateStatusMethod);
            var updateStatusAttr = updateStatusMethod.GetCustomAttribute<AuthorizeAttribute>();
            Assert.NotNull(updateStatusAttr);
            Assert.Equal("Admin", updateStatusAttr.Roles);
        }
    }
}
