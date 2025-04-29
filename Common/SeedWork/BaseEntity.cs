using System;
using System.Collections.Generic;
// Using MediatR's INotification is a common way to represent domain events,
// but you could define your own IDomainEvent interface if you prefer fewer dependencies.
using MediatR;

namespace Common.SeedWork 
{
    public abstract class BaseEntity<TId> : IEquatable<BaseEntity<TId>> where TId : IEquatable<TId>
    {
        /// <summary>
        /// Unique identifier for the entity.
        /// Setter is protected to allow ORMs/persistence layers to set it,
        //  but prevents arbitrary changes from outside.
        /// </summary>
        public virtual TId Id { get; protected set; }

        // --- Domain Events Handling ---

        private List<INotification> _domainEvents;

        /// <summary>
        /// Collection of domain events raised by this entity.
        /// Returns null if no events have been raised.
        /// </summary>
        public IReadOnlyCollection<INotification> DomainEvents => _domainEvents?.AsReadOnly();

        /// <summary>
        /// Adds a domain event to the entity's collection.
        /// </summary>
        /// <param name="eventItem">The domain event.</param>
        public void AddDomainEvent(INotification eventItem)
        {
            _domainEvents = _domainEvents ?? new List<INotification>();
            _domainEvents.Add(eventItem);
        }

        /// <summary>
        /// Removes a specific domain event from the collection.
        /// </summary>
        /// <param name="eventItem">The domain event to remove.</param>
        public void RemoveDomainEvent(INotification eventItem)
        {
            _domainEvents?.Remove(eventItem);
        }

        /// <summary>
        /// Clears all domain events from the entity.
        /// Typically called after the events have been dispatched.
        /// </summary>
        public void ClearDomainEvents()
        {
            _domainEvents?.Clear();
        }

        // --- Equality Implementation ---

        /// <summary>
        /// Checks if this entity is transient (i.e., has not been assigned an Id yet).
        /// </summary>
        /// <returns>True if the entity is transient.</returns>
        public bool IsTransient()
        {
            // Checks if the Id has the default value for its type (e.g., 0 for int, Guid.Empty for Guid)
            return EqualityComparer<TId>.Default.Equals(Id, default(TId));
        }

        /// <summary>
        /// Compares this object with another object for equality.
        /// Entities are equal if they are of the same type and have the same Id.
        /// Transient entities are only equal if they are the same instance.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is BaseEntity<TId>))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            // Must be the exact same type. Could relax this for proxies if needed.
            if (GetType() != obj.GetType())
                return false;

            BaseEntity<TId> item = (BaseEntity<TId>)obj;

            // If either entity is transient, equality is based on reference equality,
            // which we already checked wasn't true.
            if (item.IsTransient() || IsTransient())
                return false;
            else
                // Compare based on the identifier.
                return EqualityComparer<TId>.Default.Equals(item.Id, Id);
        }

        /// <summary>
        /// Gets the hash code for the entity.
        /// Uses the Id for persistent entities for consistency with Equals.
        /// Uses a changing value for transient entities.
        /// </summary>
        public override int GetHashCode()
        {
            if (!IsTransient())
            {
                // Use a stable hash code based on the Id and a prime number multiplier
                return Id.GetHashCode() ^ 31;
            }
            else
            {
                // For transient entities, the hash code is not stable.
                // Using base.GetHashCode() relies on object identity.
                return base.GetHashCode();
            }
        }

        /// <summary>
        /// Compares this entity with another entity of the same type for equality.
        /// </summary>
        public bool Equals(BaseEntity<TId> other)
        {
            return Equals((object)other);
        }

        // Operator overloads for equality comparison
        public static bool operator ==(BaseEntity<TId> left, BaseEntity<TId> right)
        {
            // Handles null checks correctly
            if (Equals(left, null))
                return Equals(right, null);
            else
                return left.Equals(right);
        }

        public static bool operator !=(BaseEntity<TId> left, BaseEntity<TId> right)
        {
            return !(left == right);
        }

        // Parameterless constructor required by some ORMs like EF Core
        protected BaseEntity()
        {
            _domainEvents = new List<INotification>();
            // Id might be set later by the database or repository
        }

        // Optional: Constructor to initialize with an Id
        protected BaseEntity(TId id) : this()
        {
             if (EqualityComparer<TId>.Default.Equals(id, default(TId)))
             {
                  throw new ArgumentException("The entity Id cannot be the default value.", nameof(id));
             }
            Id = id;
        }
    }
}