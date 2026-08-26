import { AbstractControl, FormGroup, ValidationErrors, ValidatorFn } from "@angular/forms";

export class FormUtils {
  // expresiones regulares
  static emailPattern = '^[a-z0-9._%+-]+@[a-z0-9.-]+\\.[a-z]{2,4}$';
  static usernamePattern = '^[a-zA-Z0-9_-]+$';

  static isFieldOneEqualFieldTwo( field1: string, field2: string ) {
    return ( formGroup: AbstractControl ) => {
      const field1Value = formGroup.get(field1)?.value;
      const field2Value = formGroup.get(field2)?.value;

      return field1Value === field2Value ? null : { passwordsNotEqual: true };
    }
  }

  static conditionalCurrentPasswordValidator(
    newPasswordField: string,
    currentPasswordField: string
  ) {
    return (form: AbstractControl) => {
      const newPassword = form.get(newPasswordField)?.value;
      const currentPassword = form.get(currentPasswordField)?.value;

      // Si quiere cambiar la contraseña, la actual es requerida
      if (newPassword && !currentPassword) {
        return { currentPasswordRequired: true };
      }
      return null;
    };
  }

  static isFieldOneNotEqualFieldTwo( field1: string, field2: string ): ValidatorFn {
    return (form: AbstractControl): ValidationErrors | null => {
      const field1Value = form.get(field1)?.value;
      const field2Value = form.get(field2)?.value;

      // Solo validar si ambos campos tienen valor
      if (field1Value && field2Value && field1Value === field2Value) {
        return { passwordSameAsCurrent: true };
      }
      return null;
    };
  }



  static hasVisibleError( form:FormGroup, fieldName: string ): boolean | null {
    return (
      form.controls[fieldName].errors &&  // que tenga errores
      form.controls[fieldName].touched // que el campo se haya tocado
    );
  }


  static getFieldError( form:FormGroup, fieldName: string ): string | null { // obtiene mensaje de error

    if ( !form.controls[fieldName]) { // si no existe ese control
      return null;
    }

    const errors = form.controls[fieldName].errors ?? {};

    for (const key of Object.keys(errors) ) {
      switch (key) {
        case 'required':
          return 'Este campo es requerido';

        case 'minlength':
          return `Minimo de ${ errors['minlength'].requiredLength } caracteres`;

        case 'maxlength':
          return `Maximo de ${ errors['maxlength'].requiredLength } caracteres`;

        case 'pattern':
          if( errors['pattern'].requiredPattern == FormUtils.emailPattern ){
            return 'El valor ingresado no luce como un correo electrónico';
          }

          if( errors['pattern'].requiredPattern == FormUtils.usernamePattern ){
            return 'Solo puede contener letras, números, guion bajo (_) y guion medio (-)';
          }
        break;

        case 'missingLowercase':
          return 'Debe contener al menos una letra minúscula';

        case 'missingUppercase':
          return 'Debe contener al menos una letra mayúscula';

        case 'missingDigit':
          return 'Debe contener al menos un número';

        case 'missingSpecialChar':
          return 'Debe contener al menos un carácter especial';

        case 'whitespace':
          return 'No puede contener espacios';

        case 'passwordsNotEqual':
          return 'Las contraseñas no son iguales'

        case 'currentPasswordRequired':
          return 'La contraseña actual es requerida'

      }
    }

    return null;


  }

  static passwordComplexityValidator(options?: {
    requireUppercase?: boolean;
    requireLowercase?: boolean;
    requireDigit?: boolean;
    requireSpecialChar?: boolean;
  }): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value as string;
      if (!value) return null;

      const config = {
        requireUppercase: true, // valores x defecto
        requireLowercase: true,
        requireDigit: true,
        requireSpecialChar: false,
        ...options
      };

      const errors: ValidationErrors = {};

      if (config.requireUppercase && !/[A-Z]/.test(value)) {
        errors['missingUppercase'] = true;
      }

      if (config.requireLowercase && !/[a-z]/.test(value)) {
        errors['missingLowercase'] = true;
      }

      if (config.requireDigit && !/[0-9]/.test(value)) {
        errors['missingDigit'] = true;
      }

      if (config.requireSpecialChar && !/[!@#$%^&*]/.test(value)) {
        errors['missingSpecialChar'] = true;
      }

      if (/\s/.test(value)) {
        errors['whitespace'] = true;
      }

      return Object.keys(errors).length > 0 ? errors : null;
    };
  }

}
