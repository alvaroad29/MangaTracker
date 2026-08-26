import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ApiProblemDetails } from '../../../core/auth/models/api-error.model';
import { AuthService } from '../../../core/auth/services/auth.service';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { FormUtils } from '../../../shared/form-utils';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  formUtils = FormUtils;

  loading = signal(false);
  errorMessage = signal<string | null>(null);

  loginForm = this.fb.nonNullable.group({
    email: ['', [Validators.required,  Validators.pattern( FormUtils.emailPattern )]],
    password: ['', [Validators.required, Validators.minLength(8)]]
  });

  submit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    this.auth.login(this.loginForm.getRawValue()).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigateByUrl('/search');
      },
      error: (err: { error: ApiProblemDetails }) => {
        this.loading.set(false);
        this.errorMessage.set(err.error?.title ?? 'No pudimos iniciar sesión. Probá de nuevo.');
      }
    });
  }
}
