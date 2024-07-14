using FluentValidation.Results;
using Journey.Communication.Requests;
using Journey.Communication.Responses;
using Journey.Exception.ExceptionsBase;
using Journey.Infrastructure;
using Journey.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Journey.Application.UseCases.Activities.Register
{
    public class RegisterActivityForTripUseCase
    {
        public ResponseActivityJson Execute(Guid tripId, RequestRegisterActivityJson request) 
        {
            var dbContext = new JourneyDbContext();

            var trip = dbContext
                .Trips
                .Include(trip => trip.Activities)
                .FirstOrDefault(trip => trip.Id == tripId);

            Validate(trip, request);

            var entity = new Activity
            {
                Date = request.Date,
                Name = request.Name,
                TripId = tripId,
            };

            trip.Activities.Add(entity);

            dbContext.Activities.Add(entity);
            dbContext.SaveChanges();

            return new ResponseActivityJson
            {
                Date = entity.Date,
                Name = entity.Name,
                Id = entity.Id,
                Status = (Communication.Enums.ActivityStatus)entity.Status
            };
        }

        private void Validate(Trip? trip, RequestRegisterActivityJson request)
        {
            var validator = new RegisterActivityValidator();

            var result = validator.Validate(request);

            if((request.Date >= trip.StartDate && request.Date <= trip.EndDate) == false) 
            {
                result.Errors.Add(new ValidationFailure("Date", ResourceErrorMessage.DATE_NOT_WITHIN_TRAVEL_PERIOD));
            }

            if(result.IsValid)
            {
                var errorMessages = result.Errors.Select(error => error.ErrorMessage).ToList();
                throw new ErrorOnValidationException(errorMessages);
            }
            
        }
    }
}
