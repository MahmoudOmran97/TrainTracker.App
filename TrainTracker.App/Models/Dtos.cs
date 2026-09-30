namespace TrainTracker.App.Models;

// نفس شكل TripCardDto اللي بيرجعه /api/trips/today
public record TripCard(int TripId, string TrainNumber, string? TrainType,
    string? From, string? To, TimeOnly? Departure, TimeOnly? Arrival,
    string Status, int DelayMinutes, double? LastLatitude, double? LastLongitude);
