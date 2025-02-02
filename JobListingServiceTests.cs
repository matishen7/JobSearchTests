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
    public class JobListingServiceTests
    {
        private Mock<IJobListingRepository> _mockRepository;
        private Mock<IMapper> _mockMapper;
        private Mock<ILogger<JobListingService>> _mockLogger;
        private JobListingService _jobListingService;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<IJobListingRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<JobListingService>>();
            _jobListingService = new JobListingService(_mockRepository.Object, _mockMapper.Object, _mockLogger.Object);
        }

        #region GetAllJobsAsync Tests

        [Test]
        public async Task GetAllJobsAsync_Returns_Mapped_JobListingDTOs()
        {
            var jobListings = new List<JobListing>
            {
                new JobListing { JobId = 1, Title = "Job 1" },
                new JobListing { JobId = 2, Title = "Job 2" }
            };
            var jobListingDTOs = new List<JobListingDTO>
            {
                new JobListingDTO { JobId = 1, Title = "Job 1" },
                new JobListingDTO { JobId = 2, Title = "Job 2" }
            };

            _mockRepository
                .Setup(r => r.GetAllJobsAsync())
                .ReturnsAsync(jobListings);

            _mockMapper
                .Setup(m => m.Map<List<JobListingDTO>>(jobListings))
                .Returns(jobListingDTOs);

            var result = await _jobListingService.GetAllJobsAsync();

            Assert.AreEqual(jobListingDTOs, result);
            _mockRepository.Verify(r => r.GetAllJobsAsync(), Times.Once);
        }

        [Test]
        public void GetAllJobsAsync_Throws_ApplicationException_On_Error()
        {
            _mockRepository
                .Setup(r => r.GetAllJobsAsync())
                .ThrowsAsync(new Exception("Repository error"));

            var ex = Assert.ThrowsAsync<ApplicationException>(async () => await _jobListingService.GetAllJobsAsync());
            StringAssert.Contains("An error occurred while retrieving job listings", ex.Message);
        }

        #endregion

        #region GetJobListingByIdAsync Tests

        [Test]
        public async Task GetJobListingByIdAsync_Returns_Mapped_JobListingDTO()
        {
            int jobId = 1;
            var jobListing = new JobListing { JobId = jobId, Title = "Job 1" };
            var jobListingDTO = new JobListingDTO { JobId = jobId, Title = "Job 1" };

            _mockRepository
                .Setup(r => r.GetJobByIdAsync(jobId))
                .ReturnsAsync(jobListing);

            _mockMapper
                .Setup(m => m.Map<JobListingDTO>(jobListing))
                .Returns(jobListingDTO);

            var result = await _jobListingService.GetJobListingByIdAsync(jobId);

            Assert.AreEqual(jobListingDTO, result);
            _mockRepository.Verify(r => r.GetJobByIdAsync(jobId), Times.Once);
        }

        [Test]
        public void GetJobListingByIdAsync_Throws_ApplicationException_When_NotFound()
        {
            int jobId = 1;
            _mockRepository
                .Setup(r => r.GetJobByIdAsync(jobId))
                .ReturnsAsync((JobListing)null);

            var ex = Assert.ThrowsAsync<ApplicationException>(async () => await _jobListingService.GetJobListingByIdAsync(jobId));
            StringAssert.Contains("An error occurred while retrieving job listing", ex.Message);
            Assert.IsInstanceOf<KeyNotFoundException>(ex.InnerException);
        }

        [Test]
        public void GetJobListingByIdAsync_Throws_ApplicationException_On_RepositoryError()
        {
            int jobId = 1;
            _mockRepository
                .Setup(r => r.GetJobByIdAsync(jobId))
                .ThrowsAsync(new Exception("Repository error"));

            var ex = Assert.ThrowsAsync<ApplicationException>(async () => await _jobListingService.GetJobListingByIdAsync(jobId));
            StringAssert.Contains("An error occurred while retrieving job listing", ex.Message);
        }

        #endregion

        #region AddJobListingAsync Tests

        [Test]
        public async Task AddJobListingAsync_Returns_New_JobId()
        {
            var createJobDto = new JobListingCreateDTO { Title = "New Job" };
            var jobListing = new JobListing { Title = "New Job" };
            int newJobId = 10;

            _mockMapper
                .Setup(m => m.Map<JobListing>(createJobDto))
                .Returns(jobListing);

            _mockRepository
                .Setup(r => r.AddJobAsync(jobListing))
                .ReturnsAsync(newJobId);

            var result = await _jobListingService.AddJobListingAsync(createJobDto);

            Assert.AreEqual(newJobId, result);
            _mockRepository.Verify(r => r.AddJobAsync(jobListing), Times.Once);
        }

        [Test]
        public void AddJobListingAsync_Throws_ApplicationException_When_CreateJobDto_Is_Null()
        {
            var ex = Assert.ThrowsAsync<ApplicationException>(async () => await _jobListingService.AddJobListingAsync(null));
            StringAssert.Contains("An error occurred while adding a job listing", ex.Message);
            Assert.IsInstanceOf<ArgumentNullException>(ex.InnerException);
        }

        [Test]
        public void AddJobListingAsync_Throws_ApplicationException_On_RepositoryError()
        {
            var createJobDto = new JobListingCreateDTO { Title = "New Job" };
            var jobListing = new JobListing { Title = "New Job" };

            _mockMapper
                .Setup(m => m.Map<JobListing>(createJobDto))
                .Returns(jobListing);

            _mockRepository
                .Setup(r => r.AddJobAsync(jobListing))
                .ThrowsAsync(new Exception("Repository error"));

            var ex = Assert.ThrowsAsync<ApplicationException>(async () => await _jobListingService.AddJobListingAsync(createJobDto));
            StringAssert.Contains("An error occurred while adding a job listing", ex.Message);
        }

        #endregion

        #region UpdateJobAsync Tests

        [Test]
        public async Task UpdateJobAsync_Calls_UpdateJobAsync_On_Repository()
        {
            var createJobDto = new JobListingCreateDTO { Title = "Updated Job" };
            var jobListing = new JobListing { Title = "Updated Job" };

            _mockMapper
                .Setup(m => m.Map<JobListing>(createJobDto))
                .Returns(jobListing);

            _mockRepository
                .Setup(r => r.UpdateJobAsync(jobListing))
                .Returns(Task.CompletedTask);

            await _jobListingService.UpdateJobAsync(createJobDto);

            _mockRepository.Verify(r => r.UpdateJobAsync(jobListing), Times.Once);
        }

        [Test]
        public void UpdateJobAsync_Throws_ApplicationException_When_CreateJobDto_Is_Null()
        {
            var ex = Assert.ThrowsAsync<ApplicationException>(async () => await _jobListingService.UpdateJobAsync(null));
            StringAssert.Contains("An error occurred while updating the job listing", ex.Message);
            Assert.IsInstanceOf<ArgumentNullException>(ex.InnerException);
        }

        [Test]
        public void UpdateJobAsync_Throws_ApplicationException_On_RepositoryError()
        {
            var createJobDto = new JobListingCreateDTO { Title = "Updated Job" };
            var jobListing = new JobListing { Title = "Updated Job" };

            _mockMapper
                .Setup(m => m.Map<JobListing>(createJobDto))
                .Returns(jobListing);

            _mockRepository
                .Setup(r => r.UpdateJobAsync(jobListing))
                .ThrowsAsync(new Exception("Repository error"));

            var ex = Assert.ThrowsAsync<ApplicationException>(async () => await _jobListingService.UpdateJobAsync(createJobDto));
            StringAssert.Contains("An error occurred while updating the job listing", ex.Message);
        }

        #endregion

        #region DeleteJobAsync Tests

        [Test]
        public async Task DeleteJobAsync_Calls_DeleteJobAsync_On_Repository()
        {
            int jobId = 1;
            var jobListing = new JobListing { JobId = jobId, Title = "Job 1" };

            _mockRepository
                .Setup(r => r.GetJobByIdAsync(jobId))
                .ReturnsAsync(jobListing);
            _mockRepository
                .Setup(r => r.DeleteJobAsync(jobId))
                .Returns(Task.CompletedTask);

            await _jobListingService.DeleteJobAsync(jobId);

            _mockRepository.Verify(r => r.DeleteJobAsync(jobId), Times.Once);
        }

        [Test]
        public void DeleteJobAsync_Throws_ApplicationException_When_Job_NotFound()
        {
            int jobId = 1;
            _mockRepository
                .Setup(r => r.GetJobByIdAsync(jobId))
                .ReturnsAsync((JobListing)null);

            var ex = Assert.ThrowsAsync<ApplicationException>(async () => await _jobListingService.DeleteJobAsync(jobId));
            StringAssert.Contains("An error occurred while deleting the job listing", ex.Message);
            Assert.IsInstanceOf<KeyNotFoundException>(ex.InnerException);
        }

        [Test]
        public void DeleteJobAsync_Throws_ApplicationException_On_RepositoryError()
        {
            int jobId = 1;
            var jobListing = new JobListing { JobId = jobId, Title = "Job 1" };

            _mockRepository
                .Setup(r => r.GetJobByIdAsync(jobId))
                .ReturnsAsync(jobListing);
            _mockRepository
                .Setup(r => r.DeleteJobAsync(jobId))
                .ThrowsAsync(new Exception("Repository error"));

            var ex = Assert.ThrowsAsync<ApplicationException>(async () => await _jobListingService.DeleteJobAsync(jobId));
            StringAssert.Contains("An error occurred while deleting the job listing", ex.Message);
        }

        #endregion
    }
}
