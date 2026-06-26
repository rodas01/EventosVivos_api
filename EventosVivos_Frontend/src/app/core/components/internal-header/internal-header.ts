import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-internal-header',
  imports: [],
  templateUrl: './internal-header.html',
})
export class InternalHeader {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected logout(): void {
    this.authService.logout().subscribe({
      next: () => {
        this.router.navigate(['/eventos']);
      },
      error: (err) => {
        console.error(err);
        this.authService.logoutLocal();
        this.router.navigate(['/eventos']);
      }
    });
  }
}
