import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ApiProblemDetails } from '../../../core/auth/models/api-error.model';
import { AuthService } from '../../../core/auth/services/auth.service';
import { FormUtils } from '../../../shared/form-utils';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterComponent {
  private fb = inject(FormBuilder); // para construir el form

  private auth = inject(AuthService);

  private router = inject(Router);

  formUtils = FormUtils; // para validaciones



  registerForm: FormGroup = this.fb.group({
    displayName: [
      '',
      [
        Validators.required,
        Validators.maxLength(100),
      ]
    ],
    email: [
      '',
      [
        Validators.required,
        // Validators.maxLength(254),
        Validators.pattern( FormUtils.emailPattern )
      ]
    ],
    password: [
      '',
      [
        Validators.required,
        Validators.minLength(8),
        // Validators.maxLength(100),
        FormUtils.passwordComplexityValidator({
          requireUppercase: true,
          requireLowercase: true,
          requireDigit: true
        })
      ]
    ]

  })


  loading = signal(false);
  errorMessage = signal<string | null>(null);


  submit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    this.auth.register(this.registerForm.getRawValue()).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigateByUrl('/search');
      },
      error: (err: { error: ApiProblemDetails }) => {

        this.loading.set(false);
        this.errorMessage.set(err.error?.title ?? 'No pudimos crear la cuenta. Probá de nuevo.');
      }
    });
  }
}
