import { Component, Input, Output, EventEmitter, OnInit, OnChanges, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login-modal',
  imports: [ReactiveFormsModule],
  templateUrl: './login-modal.html',
})
export class LoginModal implements OnInit, OnChanges {
  private readonly fb = inject(FormBuilder);
  public readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  @Input() isOpen = false;
  @Output() isOpenChange = new EventEmitter<boolean>();

  public loginForm!: FormGroup;
  public readonly isLogginIn = signal<boolean>(false);
  public readonly loginErrorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      username: ['', [Validators.required]],
      password: ['', [Validators.required]],
    });
  }

  ngOnChanges(): void {
    if (this.isOpen && this.loginForm) {
      this.loginForm.reset({ username: '', password: '' });
      this.loginErrorMessage.set(null);
    }
  }

  protected closeModal(): void {
    this.isOpen = false;
    this.isOpenChange.emit(false);
  }

  protected onLoginSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLogginIn.set(true);
    this.loginErrorMessage.set(null);

    const credentials = this.loginForm.value;
    this.authService.login(credentials).subscribe({
      next: () => {
        this.isLogginIn.set(false);
        this.closeModal();
        this.router.navigate(['/home']);
      },
      error: (err) => {
        console.error(err);
        this.isLogginIn.set(false);
        const msg = err.error?.message || err.error || 'Credenciales inválidas.';
        this.loginErrorMessage.set(msg);
      }
    });
  }
}
