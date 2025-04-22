import {
  registerDecorator,
  ValidationOptions,
  ValidationArguments,
  ValidatorConstraint,
  ValidatorConstraintInterface,
} from "class-validator";

@ValidatorConstraint({ name: "isFutureDate", async: false })
export class IsFutureDateConstraint implements ValidatorConstraintInterface {
  validate(date: string | undefined, args: ValidationArguments) {
    if (!date) return true;

    // check YYYY-MM-DD
    if (date.match(/^\d{4}-\d{2}-\d{2}$/)) {
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      const dateToCheck = new Date(date);

      // Check if the date is valid
      if (isNaN(dateToCheck.getTime())) {
        return false;
      }

      dateToCheck.setHours(0, 0, 0, 0);
      return dateToCheck >= today;
    }

    // For other formats, validate the date is valid first
    const dateToCheck = new Date(date);
    if (isNaN(dateToCheck.getTime())) {
      return false;
    }

    const now = new Date();
    return dateToCheck > now;
  }

  defaultMessage(args: ValidationArguments) {
    return `${args.property} must be a date in the future`;
  }
}

export function IsFutureDate(validationOptions?: ValidationOptions) {
  return function (object: Object, propertyName: string) {
    registerDecorator({
      name: "isFutureDate",
      target: object.constructor,
      propertyName: propertyName,
      options: validationOptions,
      constraints: [],
      validator: IsFutureDateConstraint,
    });
  };
}

@ValidatorConstraint({ name: "isAfterField", async: false })
export class IsAfterFieldConstraint implements ValidatorConstraintInterface {
  validate(date: string | Date | undefined, args: ValidationArguments) {
    // If date is undefined or null, skip validation (let @IsNotEmpty handle it)
    if (!date) return true;

    const [relatedPropertyName] = args.constraints;
    const relatedValue = (args.object as any)[relatedPropertyName];

    if (!relatedValue) return true;

    const dateToCheck = new Date(date);
    const relatedDate = new Date(relatedValue);

    return dateToCheck > relatedDate;
  }

  defaultMessage(args: ValidationArguments) {
    const [relatedPropertyName] = args.constraints;
    return `${args.property} must be later than ${relatedPropertyName}`;
  }
}

export function IsAfterField(
  property: string,
  validationOptions?: ValidationOptions
) {
  return function (object: Object, propertyName: string) {
    registerDecorator({
      name: "isAfterField",
      target: object.constructor,
      propertyName: propertyName,
      constraints: [property],
      options: validationOptions,
      validator: IsAfterFieldConstraint,
    });
  };
}

@ValidatorConstraint({ name: "isTimeAfter", async: false })
export class IsTimeAfterConstraint implements ValidatorConstraintInterface {
  validate(endTime: string | undefined, args: ValidationArguments) {
    if (!endTime) return true;

    const { object } = args;
    const [relatedPropertyName, dateProp] = args.constraints;
    const startTime = (object as any)[relatedPropertyName];

    if (!startTime) return true;

    // Check if both are in HH:MM format
    if (!startTime.match(/^\d{2}:\d{2}$/) || !endTime.match(/^\d{2}:\d{2}$/)) {
      return false;
    }

    // If we have a date property, we're checking times on the same day
    if (dateProp) {
      const date = (object as any)[dateProp];
      if (!date) return true;

      const startDateTime = new Date(`${date}T${startTime}:00`);
      const endDateTime = new Date(`${date}T${endTime}:00`);

      // Check if dates are valid
      if (isNaN(startDateTime.getTime()) || isNaN(endDateTime.getTime())) {
        return false;
      }

      return endDateTime > startDateTime;
    }

    // Simple time comparison (assuming same day)
    const [startHour, startMinute] = startTime.split(":").map(Number);
    const [endHour, endMinute] = endTime.split(":").map(Number);

    if (endHour > startHour) return true;
    if (endHour === startHour && endMinute > startMinute) return true;
    return false;
  }

  defaultMessage(args: ValidationArguments) {
    const [relatedPropertyName] = args.constraints;
    return `${args.property} must be later than ${relatedPropertyName}`;
  }
}

export function IsTimeAfter(
  property: string,
  dateProp?: string,
  validationOptions?: ValidationOptions
) {
  return function (object: Object, propertyName: string) {
    registerDecorator({
      name: "isTimeAfter",
      target: object.constructor,
      propertyName: propertyName,
      constraints: [property, dateProp],
      options: validationOptions,
      validator: IsTimeAfterConstraint,
    });
  };
}
