import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatToolbarModule, MatSidenavModule, MatListModule,
    MatIconModule, MatButtonModule, MatMenuModule
  ],
  template: `
    <mat-toolbar color="primary">
      <button mat-icon-button (click)="sidenavOpened = !sidenavOpened">
        <mat-icon>menu</mat-icon>
      </button>
      <span>Document Generator</span>
      <span class="spacer"></span>
      <button mat-icon-button [matMenuTriggerFor]="userMenu">
        <mat-icon>account_circle</mat-icon>
      </button>
      <mat-menu #userMenu="matMenu">
        <div class="user-info" mat-menu-item disabled>
          <strong>{{authService.user()?.fullName}}</strong>
          <br><small>{{authService.user()?.email}}</small>
        </div>
        <mat-divider></mat-divider>
        <button mat-menu-item (click)="authService.logout()">
          <mat-icon>logout</mat-icon>
          <span>Logout</span>
        </button>
      </mat-menu>
    </mat-toolbar>

    <mat-sidenav-container>
      <mat-sidenav [opened]="sidenavOpened" mode="side">
        <mat-nav-list>
          <a mat-list-item routerLink="/templates" routerLinkActive="active-link">
            <mat-icon matListItemIcon>description</mat-icon>
            <span matListItemTitle>Templates</span>
          </a>
          @if (authService.isAdmin()) {
            <mat-divider></mat-divider>
            <a mat-list-item routerLink="/lookups" routerLinkActive="active-link">
              <mat-icon matListItemIcon>list</mat-icon>
              <span matListItemTitle>Lookups</span>
            </a>
          }
        </mat-nav-list>
      </mat-sidenav>
      <mat-sidenav-content>
        <router-outlet></router-outlet>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: [`
    .spacer { flex: 1; }
    .user-info { padding: 8px 16px; }
    mat-sidenav-container { min-height: calc(100vh - 64px); }
    mat-sidenav { width: 240px; }
    .active-link { background: rgba(63, 81, 181, 0.1); }
  `]
})
export class LayoutComponent {
  sidenavOpened = true;
  constructor(public authService: AuthService) {}
}