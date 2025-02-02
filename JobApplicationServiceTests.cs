using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using JobSearchAppBackend.DTOs;
using JobSearchAppBackend.Interfaces;
using JobSearchAppBackend.Models;
using JobSearchAppBackend.Services;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace JobSearchAppBackend.Tests.Services
{
    [TestFixture]
    public class JobApplicationServiceTests
    {
        private Mock<IJobListingRepository> _mockJobListingRepository;
        private Mock<IJobApplicationRepository> _mockJobApplicationRepository;
        private Mock<IMapper> _mockMapper;
        private Mock<ILogger<JobApplicationService>> _mockLogger;
        private JobApplicationService _jobApplicationService;

        [SetUp]
        public void Setup()
        {
            _mockJobListingRepository = new Mock<IJobListingRepository>();
            _mockJobApplicationRepository = new Mock<IJobApplicationRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<JobApplicationService>>();

            _jobApplicationService = new JobApplicationService(
                _mockJobListingRepository.Object,
                _mockJobApplicationRepository.Object,
                _mockMapper.Object,
                _mockLogger.Object);
        }

        #region GetAllJobApplicationsAsync Tests

        [Test]
        public async Task GetAllJobApplicationsAsync_Returns_Mapped_JobApplicationDTOs()
        {
            var jobApplications = new List<JobApplication>
            {
                new JobApplication { JobApplicationId = 1 },
                new JobApplication { JobApplicationId = 2 }
            };

            var jobApplicationDTOs = new List<JobApplicationDTO>
            {
                new JobApplicationDTO { JobApplicationId = 1 },
                new JobApplicationDTO { JobApplicationId = 2 }
            };

            _mockJobApplicationRepository
                .Setup(repo => repo.GetAllJobApplicationsAsync())
                .ReturnsAsync(jobApplications);

            _mockMapper
                .Setup(mapper => mapper.Map<List<JobApplicationDTO>>(jobApplications))
                .Returns(jobApplicationDTOs);

            var result = await _jobApplicationService.GetAllJobApplicationsAsync();

            Assert.AreEqual(jobApplicationDTOs, result);
            _mockJobApplicationRepository.Verify(repo => repo.GetAllJobApplicationsAsync(), Times.Once);
        }

        [Test]
        public void GetAllJobApplicationsAsync_Throws_ApplicationException_On_Error()
        {
            _mockJobApplicationRepository
                .Setup(repo => repo.GetAllJobApplicationsAsync())
                .ThrowsAsync(new Exception("Repository error"));

            var ex = Assert.ThrowsAsync<ApplicationException>(async () => await _jobApplicationService.GetAllJobApplicationsAsync());
            StringAssert.Contains("An error occurred while retrieving job applications", ex.Message);
        }

        #endregion

        #region GetJobApplicationByIdAsync Tests

        [Test]
        public async Task GetJobApplicationByIdAsync_Returns_Mapped_JobApplicationDTO()
        {
            int jobApplicationId = 1;
            var jobApplication = new JobApplication { JobApplicationId = jobApplicationId };
            var jobApplicationDTO = new JobApplicationDTO { JobApplicationId = jobApplicationId };

            _mockJobApplicationRepository
                .Setup(repo => repo.GetJobApplicationByIdAsync(jobApplicationId))
                .ReturnsAsync(jobApplication);

            _mockMapper
                .Setup(mapper => mapper.Map<JobApplicationDTO>(jobApplication))
                .Returns(jobApplicationDTO);

            var result = await _jobApplicationService.GetJobApplicationByIdAsync(jobApplicationId);

            Assert.AreEqual(jobApplicationDTO, result);
            _mockJobApplicationRepository.Verify(repo => repo.GetJobApplicationByIdAsync(jobApplicationId), Times.Once);
        }

        [Test]
        public void GetJobApplicationByIdAsync_Throws_ApplicationException_When_NotFound()
        {
            int jobApplicationId = 1;
            _mockJobApplicationRepository
                .Setup(repo => repo.GetJobApplicationByIdAsync(jobApplicationId))
                .ReturnsAsync((JobApplication)null);

            var ex = Assert.ThrowsAsync<ApplicationException>(async () => await _jobApplicationService.GetJobApplicationByIdAsync(jobApplicationId));
            StringAssert.Contains("An error occurred while retrieving job application", ex.Message);
            Assert.IsInstanceOf<KeyNotFoundException>(ex.InnerException);
        }

        [Test]
        public void GetJobApplicationByIdAsync_Throws_ApplicationException_On_RepositoryError()
        {
            int jobApplicationId = 1;
            _mockJobApplicationRepository
                .Setup(repo => repo.GetJobApplicationByIdAsync(jobApplicationId))
                .ThrowsAsync(new Exception("Repository error"));

            var ex = Assert.ThrowsAsync<ApplicationException>(async () => await _jobApplicationService.GetJobApplicationByIdAsync(jobApplicationId));
            StringAssert.Contains("An error occurred while retrieving job application", ex.Message);
        }

        #endregion

        #region AddJobApplicationAsync Tests

        [Test]
        public async Task AddJobApplicationAsync_Returns_New_JobApplicationId()
        {
            var createDto = new JobApplicationCreateDTO { /* set properties as needed */ };
            var jobApplication = new JobApplication { /* set properties accordingly */ };
            int newId = 10;

            _mockMapper
                .Setup(mapper => mapper.Map<JobApplication>(createDto))
                .Returns(jobApplication);

            _mockJobApplicationRepository
                .Setup(repo => repo.AddJobApplicationAsync(jobApplication))
                .ReturnsAsync(newId);

            var result = await _jobApplicationService.AddJobApplicationAsync(createDto);

            Assert.AreEqual(newId, result);
            _mockJobApplicationRepository.Verify(repo => repo.AddJobApplicationAsync(jobApplication), Times.Once);
        }

        [Test]
        public void AddJobApplicationAsync_Throws_ApplicationException_When_CreateDto_Is_Null()
        {
            var ex = Assert.ThrowsAsync<ApplicationException>(async () => await _jobApplicationService.AddJobApplicationAsync(null));
            StringAssert.Contains("An error occurred while adding job application", ex.Message);
            Assert.IsInstanceOf<ArgumentNullException>(ex.InnerException);
        }

        [Test]
        public void AddJobApplicationAsync_Throws_ApplicationException_On_RepositoryError()
        {
            var createDto = new JobApplicationCreateDTO { /* set properties as needed */ };
            var jobApplication = new JobApplication { /* set properties accordingly */ };

            _mockMapper
                .Setup(mapper => mapper.Map<JobApplication>(createDto))
                .Returns(jobApplication);

            _mockJobApplicationRepository
                .Setup(repo => repo.AddJobApplicationAsync(jobApplication))
                .ThrowsAsync(new Exception("Repository error"));

            var ex = Assert.ThrowsAsync<ApplicationException>(async () => await _jobApplicationService.AddJobApplicationAsync(createDto));
            StringAssert.Contains("An error occurred while adding job application", ex.Message);
        }

        #endregion

        #region UpdateJobApplicationAsync Tests

        [Test]
        public async Task UpdateJobApplicationAsync_Calls_Update_On_Repository()
        {
            var createDto = new JobApplicationCreateDTO { /* set properties as needed */ };
            var jobApplication = new JobApplication { /* set properties accordingly */ };

            _mockMapper
                .Setup(mapper => mapper.Map<JobApplication>(createDto))
                .Returns(jobApplication);

            _mockJobApplicationRepository
                .Setup(repo => repo.UpdateJobApplicationAsync(jobApplication))
                .Returns(Task.CompletedTask);

            await _jobApplicationService.UpdateJobApplicationAsync(createDto);

            _mockJobApplicationRepository.Verify(repo => repo.UpdateJobApplicationAsync(jobApplication), Times.Once);
        }

        [Test]
        public void UpdateJobApplicationAsync_Throws_ApplicationException_When_CreateDto_Is_Null()
        {
            var ex = Assert.ThrowsAsync<ApplicationException>(async () => await _jobApplicationService.UpdateJobApplicationAsync(null));
            StringAssert.Contains("An error occurred while updating job application", ex.Message);
            Assert.IsInstanceOf<ArgumentNullException>(ex.InnerException);
        }

        [Test]
        public void UpdateJobApplicationAsync_Throws_ApplicationException_On_RepositoryError()
        {
            var createDto = new JobApplicationCreateDTO { /* set properties as needed */ };
            var jobApplication = new JobApplication { /* set properties accordingly */ };

            _mockMapper
                .Setup(mapper => mapper.Map<JobApplication>(createDto))
                .Returns(jobApplication);

            _mockJobApplicationRepository
                .Setup(repo => repo.UpdateJobApplicationAsync(jobApplication))
                .ThrowsAsync(new Exception("Repository error"));

            var ex = Assert.ThrowsAsync<ApplicationException>(async () => await _jobApplicationService.UpdateJobApplicationAsync(createDto));
            StringAssert.Contains("An error occurred while updating job application", ex.Message);
        }

        #endregion

        #region DeleteJobApplicationAsync Tests

        [Test]
        public async Task DeleteJobApplicationAsync_Calls_Delete_On_Repository()
        {
            int jobApplicationId = 1;
            var jobApplication = new JobApplication { JobApplicationId = jobApplicationId };

            _mockJobApplicationRepository
                .Setup(repo => repo.GetJobApplicationByIdAsync(jobApplicationId))
                .ReturnsAsync(jobApplication);

            _mockJobApplicationRepository
                .Setup(repo => repo.DeleteJobApplicationAsync(jobApplicationId))
                .Returns(Task.CompletedTask);

            await _jobApplicationService.DeleteJobApplicationAsync(jobApplicationId);

            _mockJobApplicationRepository.Verify(repo => repo.DeleteJobApplicationAsync(jobApplicationId), Times.Once);
        }

        [Test]
        public void DeleteJobApplicationAsync_Throws_ApplicationException_When_JobApplication_NotFound()
        {
            int jobApplicationId = 1;
            _mockJobApplicationRepository
                .Setup(repo => repo.GetJobApplicationByIdAsync(jobApplicationId))
                .ReturnsAsync((JobApplication)null);

            var ex = Assert.ThrowsAsync<ApplicationException>(async () => await _jobApplicationService.DeleteJobApplicationAsync(jobApplicationId));
            StringAssert.Contains("An error occurred while deleting job application", ex.Message);
            Assert.IsInstanceOf<KeyNotFoundException>(ex.InnerException);
        }

        [Test]
        public void DeleteJobApplicationAsync_Throws_ApplicationException_On_RepositoryError()
        {
            int jobApplicationId = 1;
            var jobApplication = new JobApplication { JobApplicationId = jobApplicationId };

            _mockJobApplicationRepository
                .Setup(repo => repo.GetJobApplicationByIdAsync(jobApplicationId))
                .ReturnsAsync(jobApplication);

            _mockJobApplicationRepository
                .Setup(repo => repo.DeleteJobApplicationAsync(jobApplicationId))
                .ThrowsAsync(new Exception("Repository error"));

            var ex = Assert.ThrowsAsync<ApplicationException>(async () => await _jobApplicationService.DeleteJobApplicationAsync(jobApplicationId));
            StringAssert.Contains("An error occurred while deleting job application", ex.Message);
        }

        #endregion
    }
}
