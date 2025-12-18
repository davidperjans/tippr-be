namespace Application.Common
{
    /// <summary>
    /// Represents the semantic category of an application error.
    /// Used to determine HTTP status codes and error response behavior.
    /// </summary>
    public enum ErrorType
    {
        /// <summary>
        /// The request contains invalid input data.
        ///
        /// Use when:
        /// - Required fields are missing
        /// - Invalid formats or values are provided
        /// - FluentValidation rules fail
        ///
        /// HTTP: 400 Bad Request
        ///
        /// Examples:
        /// - Prediction score is negative
        /// - Invite code has wrong length
        /// - Required name field is empty
        ///
        /// Example code:
        /// - "validation.failed"
        /// - "prediction.score.invalid"
        /// </summary>
        Validation = 1,

        /// <summary>
        /// The requested resource could not be found.
        ///
        /// Use when:
        /// - An entity with the given identifier does not exist
        /// - A required related resource is missing
        ///
        /// HTTP: 404 Not Found
        ///
        /// Examples:
        /// - League not found
        /// - Match not found
        /// - Prediction not found
        ///
        /// Example code:
        /// - "league.not_found"
        /// - "match.not_found"
        /// </summary>
        NotFound = 2,

        /// <summary>
        /// The user is not authenticated.
        ///
        /// Use when:
        /// - Authentication token is missing or invalid
        /// - User identity cannot be resolved
        ///
        /// HTTP: 401 Unauthorized
        ///
        /// Note:
        /// This is often handled by authentication middleware
        /// rather than application handlers.
        ///
        /// Example code:
        /// - "auth.unauthorized"
        /// </summary>
        Unauthorized = 3,

        /// <summary>
        /// The user is authenticated but not authorized to perform the action.
        ///
        /// Use when:
        /// - User attempts to modify a resource they do not own
        /// - User lacks required role or permission
        ///
        /// HTTP: 403 Forbidden
        ///
        /// Examples:
        /// - User tries to update another user's prediction
        /// - Non-owner attempts to delete a league
        ///
        /// Example code:
        /// - "prediction.forbidden"
        /// - "league.forbidden"
        /// </summary>
        Forbidden = 4,

        /// <summary>
        /// The request conflicts with the current state of the system.
        ///
        /// Use when:
        /// - A unique constraint would be violated
        /// - A resource already exists
        ///
        /// HTTP: 409 Conflict
        ///
        /// Examples:
        /// - User already joined the league
        /// - Prediction already exists for a match
        /// - Invite code already taken
        ///
        /// Example code:
        /// - "league.already_member"
        /// - "prediction.already_exists"
        /// </summary>
        Conflict = 5,

        /// <summary>
        /// The request violates a business rule despite having valid input.
        ///
        /// Use when:
        /// - The action is not allowed due to domain rules
        /// - The system state prevents the operation
        ///
        /// HTTP: 400 Bad Request
        ///
        /// Examples:
        /// - League is full
        /// - Prediction deadline has passed
        /// - Transfers are locked
        ///
        /// Example code:
        /// - "league.full"
        /// - "prediction.deadline_passed"
        /// </summary>
        BusinessRule = 6,

        /// <summary>
        /// A generic failure with no specific semantic classification.
        ///
        /// Use when:
        /// - The error does not yet have a defined category
        /// - Legacy or temporary fallback behavior
        ///
        /// HTTP: 400 Bad Request
        ///
        /// Note:
        /// This should be avoided in favor of more specific error types.
        ///
        /// Example code:
        /// - "request.failed"
        /// </summary>
        Failure = 7
    }
}
