import { Component, signal, inject, OnInit } from '@angular/core';
import { RouterOutlet, Router, ActivatedRoute, NavigationEnd } from '@angular/router';
import { Header } from './core/components/header/header';
import { Footer } from './core/components/footer/footer';
import { filter } from 'rxjs';
import { InternalHeader } from './core/components/internal-header/internal-header';
import { InternalMenu } from './core/components/internal-menu/internal-menu';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Header, Footer, InternalHeader, InternalMenu],
  templateUrl: './app.html',
})
export class App implements OnInit {
  private readonly router = inject(Router);
  private readonly activatedRoute = inject(ActivatedRoute);

  protected readonly title = signal('EventosVivos_Frontend');
  public readonly internal = signal<boolean>(false);
  public readonly showHeader = signal<boolean>(true);
  public readonly showFooter = signal<boolean>(true);

  ngOnInit(): void {
    this.updateVisibility();

    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe(() => {
        this.updateVisibility();
      });
  }

  private updateVisibility(): void {
    let route = this.activatedRoute;
    while (route.firstChild) {
      route = route.firstChild;
    }
    const data = route.snapshot.data;
    this.internal.set(data['internal'] === true);
    this.showHeader.set(data['showHeader'] !== false);
    this.showFooter.set(data['showFooter'] !== false);
  }
}
