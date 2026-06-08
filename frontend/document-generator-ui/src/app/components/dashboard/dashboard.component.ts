import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, MatCardModule, MatButtonModule, MatIconModule],
  template: `
    <div class="dashboard-container">
      <h1>Dashboard</h1>
      <p>Welcome back, {{authService.user()?.fullName}}!</p>

      <div class="stats-grid">
        <mat-card class="stat-card" routerLink="/templates">
          <mat-card-content>
            <mat-icon>description</mat-icon>
            <h3>Templates</h3>
            <p>Manage document templates</p>
          </mat-card-content>
        </mat-card>

        <mat-card class="stat-card" routerLink="/generate">
          <mat-card-content>
            <mat-icon>picture_as_pdf</mat-icon>
            <h3>Generate Document</h3>
            <p>Create PDF documents</p>
          </mat-card-content>
        </mat-card>

        <mat-card class="stat-card" routerLink="/history">
          <mat-card-content>
            <mat-icon>history</mat-icon>
            <h3>Document History</h3>
            <p>View generated documents</p>
          </mat-card-content>
        </mat-card>

        @if (authService.isAdmin()) {
          <mat-card class="stat-card" routerLink="/lookups">
            <mat-card-content>
              <mat-icon>list</mat-icon>
              <h3>Lookups</h3>
              <p>Manage dropdown lists</p>
            </mat-card-content>
          </mat-card>
        }
      </div>
    </div>
  `,
  styles: [`
    .dashboard-container { padding: 24px; }
    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
      gap: 16px;
      margin-top: 24px;
    }
    .stat-card {
      cursor: pointer;
      transition: transform 0.2s, box-shadow 0.2s;
    }
    .stat-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    }
    .stat-card mat-icon { font-size: 48px; width: 48px; height: 48px; color: #3f51b5; }
    .stat-card h3 { margin: 12px 0 4px; }
    .stat-card p { color: #666; margin: 0; }
  `]
})
export class DashboardComponent {
  constructor(public authService: AuthService) {}
}