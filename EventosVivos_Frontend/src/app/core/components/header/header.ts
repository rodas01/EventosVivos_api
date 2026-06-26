import { Component, signal, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { LoginModal } from '../login-modal/login-modal';

@Component({
  selector: 'app-header',
  imports: [RouterLinkActive, RouterLink, LoginModal],
  templateUrl: './header.html',
})
export class Header {
  public readonly authService = inject(AuthService);
  public readonly showLoginModal = signal<boolean>(false);

  protected openLoginModal(): void {
    this.showLoginModal.set(true);
  }

  protected logout(): void {
    this.authService.logout().subscribe({
      next: () => {
        // Logout successful
      },
      error: (err) => {
        console.error(err);
        this.authService.logoutLocal();
      }
    });
  }
}

