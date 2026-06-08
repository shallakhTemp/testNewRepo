import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TemplateService, Template } from '../../services/template.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-templates',
  standalone: true,
  imports: [
    CommonModule, RouterModule, FormsModule,
    MatTableModule, MatPaginatorModule, MatButtonModule,
    MatIconModule, MatFormFieldModule, MatInputModule,
    MatSlideToggleModule, MatDialogModule, MatSnackBarModule
  ],
  template: `
    <div class="container">
      <h1>Templates</h1>

      <div class="toolbar">
        <mat-form-field appearance="outline">
          <mat-label>Search</mat-label>
          <input matInput (input)="onSearch($event)" placeholder="Search templates...">
          <mat-icon matPrefix>search</mat-icon>
        </mat-form-field>

        @if (authService.isAdmin()) {
          <button mat-raised-button color="primary" (click)="showCreateDialog = true">
            <mat-icon>add</mat-icon> New Template
          </button>
        }
      </div>

      <!-- Create Template Dialog -->
      @if (showCreateDialog) {
        <div class="dialog-overlay" (click)="showCreateDialog = false">
          <div class="dialog" (click)="$event.stopPropagation()">
            <h3>Create New Template</h3>
            <div class="form-group">
              <label>Code</label>
              <input [(ngModel)]="newCode" placeholder="e.g. DOC-001" />
            </div>
            <div class="form-group">
              <label>Name</label>
              <input [(ngModel)]="newName" placeholder="e.g. Official Document" />
            </div>
            <div class="form-group">
              <label>Description</label>
              <textarea [(ngModel)]="newDescription" placeholder="Optional description" rows="2"></textarea>
            </div>
            <div class="dialog-actions">
              <button class="btn" (click)="showCreateDialog = false">Cancel</button>
              <button class="btn btn-primary" (click)="createTemplate()" [disabled]="!newCode || !newName">Create & Design</button>
            </div>
          </div>
        </div>
      }

      <table mat-table [dataSource]="templates()">
        <ng-container matColumnDef="code">
          <th mat-header-cell *matHeaderCellDef>Code</th>
          <td mat-cell *matCellDef="let t">{{t.code}}</td>
        </ng-container>

        <ng-container matColumnDef="name">
          <th mat-header-cell *matHeaderCellDef>Name</th>
          <td mat-cell *matCellDef="let t">{{t.name}}</td>
        </ng-container>

        <ng-container matColumnDef="description">
          <th mat-header-cell *matHeaderCellDef>Description</th>
          <td mat-cell *matCellDef="let t">{{t.description || '-'}}</td>
        </ng-container>

        <ng-container matColumnDef="isActive">
          <th mat-header-cell *matHeaderCellDef>Status</th>
          <td mat-cell *matCellDef="let t">
            <span [class.active]="t.isActive" [class.inactive]="!t.isActive">
              {{t.isActive ? 'Active' : 'Inactive'}}
            </span>
          </td>
        </ng-container>

        <ng-container matColumnDef="actions">
          <th mat-header-cell *matHeaderCellDef>Actions</th>
          <td mat-cell *matCellDef="let t">
            @if (authService.isAdmin()) {
              <button mat-icon-button (click)="designTemplate(t)" matTooltip="Design">
                <mat-icon>design_services</mat-icon>
              </button>
              <button mat-icon-button (click)="toggleActive(t)" matTooltip="Toggle Active">
                <mat-icon>{{t.isActive ? 'toggle_on' : 'toggle_off'}}</mat-icon>
              </button>
              <button mat-icon-button color="warn" (click)="deleteTemplate(t)" matTooltip="Delete">
                <mat-icon>delete</mat-icon>
              </button>
            }
            @if (!authService.isAdmin() && t.isActive) {
              <button mat-raised-button color="primary" (click)="fillTemplate(t)">
                <mat-icon>edit_note</mat-icon> Fill & Print
              </button>
            }
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
      </table>

      @if (templates().length === 0) {
        <div class="empty-state">
          <p>No templates found.</p>
          @if (authService.isAdmin()) {
            <button mat-stroked-button (click)="showCreateDialog = true">Create your first template</button>
          }
        </div>
      }

      <mat-paginator [length]="totalCount()" [pageSize]="pageSize()" [pageIndex]="pageIndex()"
        (page)="onPageChange($event)" [pageSizeOptions]="[5, 10, 25]">
      </mat-paginator>
    </div>
  `,
  styles: [`
    .container { padding: 24px; }
    .toolbar { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
    table { width: 100%; }
    .active { color: #4caf50; font-weight: bold; }
    .inactive { color: #f44336; font-weight: bold; }
    mat-form-field { width: 300px; }
    .empty-state { text-align: center; padding: 40px; color: #999; }
    
    .dialog-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0.4); display: flex; align-items: center; justify-content: center; z-index: 1000; }
    .dialog { background: #fff; border-radius: 8px; padding: 24px; width: 400px; box-shadow: 0 8px 32px rgba(0,0,0,0.2); }
    .dialog h3 { margin: 0 0 16px; font-size: 18px; }
    .form-group { margin-bottom: 12px; }
    .form-group label { display: block; font-size: 13px; font-weight: 500; color: #555; margin-bottom: 4px; }
    .form-group input, .form-group textarea { width: 100%; padding: 8px; border: 1px solid #ddd; border-radius: 4px; font-size: 14px; box-sizing: border-box; }
    .dialog-actions { display: flex; gap: 8px; justify-content: flex-end; margin-top: 16px; }
    .btn { padding: 8px 16px; border: 1px solid #ccc; border-radius: 4px; background: #fff; cursor: pointer; font-size: 14px; }
    .btn-primary { background: #1976d2; color: #fff; border-color: #1976d2; }
    .btn:disabled { opacity: 0.5; cursor: default; }
  `]
})
export class TemplatesComponent implements OnInit {
  templates = signal<Template[]>([]);
  totalCount = signal(0);
  pageIndex = signal(0);
  pageSize = signal(10);
  searchTerm = '';
  displayedColumns = ['code', 'name', 'description', 'isActive', 'actions'];
  
  showCreateDialog = false;
  newCode = '';
  newName = '';
  newDescription = '';

  constructor(
    private templateService: TemplateService,
    public authService: AuthService,
    private snackBar: MatSnackBar,
    private router: Router
  ) {}

  ngOnInit(): void { this.loadTemplates(); }

  loadTemplates(): void {
    this.templateService.getTemplates(this.pageIndex() + 1, this.pageSize(), this.searchTerm)
      .subscribe(res => {
        if (res.success && res.data) {
          this.templates.set(res.data.items);
          this.totalCount.set(res.data.totalCount);
        }
      });
  }

  onSearch(event: Event): void {
    this.searchTerm = (event.target as HTMLInputElement).value;
    this.pageIndex.set(0);
    this.loadTemplates();
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.loadTemplates();
  }

  createTemplate(): void {
    if (!this.newCode || !this.newName) return;
    this.templateService.createTemplate({
      code: this.newCode,
      name: this.newName,
      description: this.newDescription || undefined,
      isActive: true,
      designJson: '{}'
    }).subscribe(res => {
      if (res.success && res.data) {
        this.showCreateDialog = false;
        this.newCode = '';
        this.newName = '';
        this.newDescription = '';
        this.router.navigate(['/templates/design', res.data.id]);
      }
    });
  }

  designTemplate(template: Template): void {
    this.router.navigate(['/templates/design', template.id]);
  }

  fillTemplate(template: Template): void {
    this.router.navigate(['/templates/fill', template.id]);
  }

  toggleActive(template: Template): void {
    this.templateService.toggleActive(template.id).subscribe(res => {
      if (res.success) {
        this.snackBar.open('Template status updated', 'Close', { duration: 3000 });
        this.loadTemplates();
      }
    });
  }

  deleteTemplate(template: Template): void {
    if (confirm(`Delete template "${template.name}"?`)) {
      this.templateService.deleteTemplate(template.id).subscribe(res => {
        if (res.success) {
          this.snackBar.open('Template deleted', 'Close', { duration: 3000 });
          this.loadTemplates();
        }
      });
    }
  }
}