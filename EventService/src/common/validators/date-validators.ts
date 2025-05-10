import {
  registerDecorator,
  ValidationOptions,
  ValidationArguments,
  ValidatorConstraint,
  ValidatorConstraintInterface,
} from "class-validator";

@ValidatorConstraint({ name: "isFutureDate", async: false })
export class IsFutureDateConstraint implements ValidatorConstraintInterface {
  validate(dateTime: string | undefined, args: ValidationArguments) {
    if (!dateTime) return true;

    const dateToCheck = new Date(dateTime);

    if (isNaN(dateToCheck.getTime())) {
      return false;
    }

    const now = new Date();
    if (dateTime.match(/^\d{4}-\d{2}-\d{2}$/)) {
      now.setHours(0, 0, 0, 0);
      dateToCheck.setHours(0, 0, 0, 0);
      return dateToCheck >= now;
    }

    return dateToCheck > now;
  }

  defaultMessage(args: ValidationArguments) {
    return `${args.property} must be a datetime in the future`;
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

@ValidatorConstraint({ name: "isDateTimeAfter", async: false })
export class IsDateTimeAfterConstraint implements ValidatorConstraintInterface {
  validate(endDateTime: string | undefined, args: ValidationArguments) {
    if (!endDateTime) return true;

    const { object } = args;
    const [relatedPropertyName] = args.constraints;
    const startDateTime = (object as any)[relatedPropertyName];

    if (!startDateTime) return true;

    const startDate = new Date(startDateTime);
    const endDate = new Date(endDateTime);

    if (isNaN(startDate.getTime()) || isNaN(endDate.getTime())) {
      return false;
    }

    return endDate > startDate;
  }

  defaultMessage(args: ValidationArguments) {
    const [relatedPropertyName] = args.constraints;
    return `${args.property} must be later than ${relatedPropertyName}`;
  }
}

export function IsDateTimeAfter(
  property: string,
  validationOptions?: ValidationOptions
) {
  return function (object: Object, propertyName: string) {
    registerDecorator({
      name: "isDateTimeAfter",
      target: object.constructor,
      propertyName: propertyName,
      constraints: [property],
      options: validationOptions,
      validator: IsDateTimeAfterConstraint,
    });
  };
}
