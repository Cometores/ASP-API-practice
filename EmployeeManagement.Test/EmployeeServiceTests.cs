﻿using EmployeeManagement.Business;
using EmployeeManagement.Business.EventArguments;
using EmployeeManagement.Business.Exceptions;
using EmployeeManagement.DataAccess.Entities;
using EmployeeManagement.Test.Fixtures;
using EmployeeManagement.Test.Services;
using Xunit.Abstractions;

namespace EmployeeManagement.Test;

/* Implements Collection Fixture "sharing context" feature together with DataDrivenEmployeeServiceTests */
[Collection("EmployeeServiceCollection")]
public class EmployeeServiceTests //: IClassFixture<EmployeeServiceFixture>
{
    private readonly EmployeeServiceFixture _employeeServiceFixture;
    private readonly ITestOutputHelper _testOutputHelper;

    public EmployeeServiceTests(EmployeeServiceFixture employeeServiceFixture, ITestOutputHelper testOutputHelper)
    {
        _employeeServiceFixture = employeeServiceFixture;
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void CreateInternalEmployee_InternalEmployeeCreated_MustHaveAttendedFirstObligatoryCourse_WithObject()
    {
        // Arrange
        var obligatoryCourse =
            _employeeServiceFixture.EmployeeManagementTestDataRepository.GetCourse(
                Guid.Parse("37e03ca7-c730-4351-834c-b66f280cdb01"));

        // Act
        var internalEmployee = _employeeServiceFixture.EmployeeService.CreateInternalEmployee("Brooklyn", "Cannon");

        // Using ITestOutputHelper to Output Additional Information
        _testOutputHelper.WriteLine($"Employee after Act: " +
                                    $"{internalEmployee.FirstName} {internalEmployee.LastName}");
        internalEmployee.AttendedCourses.ForEach(c => _testOutputHelper.WriteLine($"Attended course: {c.Id} {c.Title}"));

        // Assert
        Assert.Contains(obligatoryCourse, internalEmployee.AttendedCourses);
    }

    [Fact]
    public void CreateInternalEmployee_InternalEmployeeCreated_MustHaveAttendedFirstObligatoryCourse_WithPredicate()
    {
        // Act
        var internalEmployee = _employeeServiceFixture.EmployeeService.CreateInternalEmployee("Brooklyn", "Cannon");

        // Assert
        Assert.Contains(internalEmployee.AttendedCourses,
            course => course.Id == Guid.Parse("37e03ca7-c730-4351-834c-b66f280cdb01"));
    }

    [Fact]
    public void CreateInternalEmployee_InternalEmployeeCreated_MustHaveAttendedSecondObligatoryCourse_WithPredicate()
    {
        // Act
        var internalEmployee = _employeeServiceFixture.EmployeeService.CreateInternalEmployee("Brooklyn", "Cannon");

        // Assert
        Assert.Contains(internalEmployee.AttendedCourses,
            course => course.Id == Guid.Parse("1fd115cf-f44c-4982-86bc-a8fe2e4ff83e"));
    }

    [Fact]
    public void CreateInternalEmployee_InternalEmployeeCreated_AttendedCoursesMustMatchObligatoryCourses()
    {
        // Arrange
        var obligatoryCourses =
            _employeeServiceFixture.EmployeeManagementTestDataRepository.GetCourses(
                Guid.Parse("37e03ca7-c730-4351-834c-b66f280cdb01"),
                Guid.Parse("1fd115cf-f44c-4982-86bc-a8fe2e4ff83e"));
        // Act
        var internalEmployee = _employeeServiceFixture.EmployeeService.CreateInternalEmployee("Brooklyn", "Cannon");

        // Assert
        Assert.Equal(obligatoryCourses, internalEmployee.AttendedCourses);
    }

    [Fact]
    public void CreateInternalEmployee_InternalEmployeeCreated_AttendedCoursesMustNotBeNew()
    {
        // Act
        var internalEmployee = _employeeServiceFixture.EmployeeService.CreateInternalEmployee("Brooklyn", "Cannon");

        // Assert
        // foreach (var course in internalEmployee.AttendedCourses)
        // {
        //     Assert.False(course.IsNew);
        // }
        Assert.All(internalEmployee.AttendedCourses, course => Assert.False(course.IsNew));
    }

    /* Testing async code */
    [Fact]
    public async Task CreateInternalEmployee_InternalEmployeeCreated_AttendedCoursesMustMatchObligatoryCourses_Async()
    {
        // Arrange
        var obligatoryCourses = await _employeeServiceFixture.EmployeeManagementTestDataRepository.GetCoursesAsync(
            Guid.Parse("37e03ca7-c730-4351-834c-b66f280cdb01"),
            Guid.Parse("1fd115cf-f44c-4982-86bc-a8fe2e4ff83e"));

        // Act
        var internalEmployee =
            await _employeeServiceFixture.EmployeeService.CreateInternalEmployeeAsync("Brooklyn", "Cannon");

        // Assert
        Assert.Equal(obligatoryCourses, internalEmployee.AttendedCourses);
    }

    /* Async exception handling */
    [Fact]
    public async Task GiveRaise_RaiseBelowMinimumGiven_EmployeeInvalidRaiseExceptionMustBeThrown()
    {
        // Creating employee not with Factory or Service - to separate logic we are not testing (logic for creation)
        var internalEmployee = new InternalEmployee("Brooklyn", "Cannon", 5, 3000, false, 1);

        // Act & Assert
        await Assert.ThrowsAsync<EmployeeInvalidRaiseException>(
            async () =>
                await _employeeServiceFixture.EmployeeService.GiveRaiseAsync(internalEmployee, 50));
    }

    /* Testing events */
    [Fact]
    public void NotifyOfAbsence_EmployeeIsAbsent_OnEmployeeIsAbsentMustBeTriggered()
    {
        // Arrange
        var internalEmployee = new InternalEmployee("Brooklyn", "Cannon", 5, 3000, false, 1);

        // Act & Assert
        Assert.Raises<EmployeeIsAbsentEventArgs>(
            handler => _employeeServiceFixture.EmployeeService.EmployeeIsAbsent += handler,
            handler => _employeeServiceFixture.EmployeeService.EmployeeIsAbsent -= handler,
            () => _employeeServiceFixture.EmployeeService.NotifyOfAbsence(internalEmployee));
    }
}