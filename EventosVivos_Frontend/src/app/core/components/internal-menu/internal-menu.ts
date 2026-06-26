import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-internal-menu',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './internal-menu.html',
})
export class InternalMenu {}
