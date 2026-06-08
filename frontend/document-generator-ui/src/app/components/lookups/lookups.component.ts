import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TemplateService, Lookup, LookupItem } from '../../services/template.service';

@Component({
  selector: 'app-lookups',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule,
    MatCardModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatListModule,
    MatExpansionModule, MatSnackBarModule
  ],
  template: `
    <div class="container">
      <h1>Lookups</h1>

      <mat-card class="create-card">
        <mat-card-content>
          <h3>Create New Lookup</h3>
          <div class="form-row">
            <mat-form-field appearance="outline">
              <mat-label>Name</mat-label>
              <input matInput [(ngModel)]="newLookup.name" placeholder="e.g. Departments">
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Description</mat-label>
              <input matInput [(ngModel)]="newLookup.description">
            </mat-form-field>
            <button mat-raised-button color="primary" (click)="createLookup()">
              <mat-icon>add</mat-icon> Create
            </button>
          </div>
        </mat-card-content>
      </mat-card>

      <mat-accordion>
        @for (lookup of lookups(); track lookup.id) {
          <mat-expansion-panel>
            <mat-expansion-panel-header>
              <mat-panel-title>{{lookup.name}}</mat-panel-title>
              <mat-panel-description>{{lookup.items?.length || 0}} items</mat-panel-description>
            </mat-expansion-panel-header>

            <div class="items-section">
              <h4>Items</h4>
              <div class="form-row">
                <mat-form-field appearance="outline">
                  <mat-label>Value</mat-label>
                  <input matInput [(ngModel)]="newItemValue" placeholder="hr">
                </mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>Text</mat-label>
                  <input matInput [(ngModel)]="newItemText" placeholder="Human Resources">
                </mat-form-field>
                <button mat-raised-button color="accent" (click)="addItem(lookup.id)">
                  <mat-icon>add</mat-icon> Add Item
                </button>
              </div>

              <mat-list>
                @for (item of lookup.items || []; track item.id) {
                  <mat-list-item>
                    <span matListItemTitle>{{item.text}}</span>
                    <span matListItemLine>Value: {{item.value}}</span>
                    <button mat-icon-button color="warn" matListItemMeta (click)="deleteItem(item)">
                      <mat-icon>delete</mat-icon>
                    </button>
                  </mat-list-item>
                }
              </mat-list>

              <button mat-raised-button color="warn" (click)="deleteLookup(lookup)" class="delete-btn">
                <mat-icon>delete</mat-icon> Delete Lookup
              </button>
            </div>
          </mat-expansion-panel>
        }
      </mat-accordion>
    </div>
  `,
  styles: [`
    .container { padding: 24px; }
    .create-card { margin-bottom: 24px; }
    .form-row { display: flex; gap: 16px; align-items: center; flex-wrap: wrap; }
    .form-row mat-form-field { flex: 1; min-width: 200px; }
    .items-section { padding: 8px 0; }
    .delete-btn { margin-top: 16px; }
  `]
})
export class LookupsComponent implements OnInit {
  lookups = signal<Lookup[]>([]);
  newLookup: Partial<Lookup> = {};
  newItemValue = '';
  newItemText = '';

  constructor(
    private templateService: TemplateService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void { this.loadLookups(); }

  loadLookups(): void {
    this.templateService.getLookups(1, 100).subscribe(res => {
      if (res.success && res.data) {
        this.lookups.set(res.data.items);
      }
    });
  }

  createLookup(): void {
    if (!this.newLookup.name) {
      this.snackBar.open('Name is required', 'Close', { duration: 3000 });
      return;
    }
    this.templateService.createLookup(this.newLookup).subscribe(res => {
      if (res.success) {
        this.snackBar.open('Lookup created', 'Close', { duration: 3000 });
        this.newLookup = {};
        this.loadLookups();
      }
    });
  }

  addItem(lookupId: string): void {
    if (!this.newItemValue || !this.newItemText) {
      this.snackBar.open('Value and Text are required', 'Close', { duration: 3000 });
      return;
    }
    this.templateService.createLookupItem({
      lookupId, value: this.newItemValue, text: this.newItemText, displayOrder: 0
    }).subscribe(res => {
      if (res.success) {
        this.snackBar.open('Item added', 'Close', { duration: 3000 });
        this.newItemValue = '';
        this.newItemText = '';
        this.loadLookups();
      }
    });
  }

  deleteItem(item: LookupItem): void {
    this.templateService.deleteLookupItem(item.id).subscribe(res => {
      if (res.success) {
        this.snackBar.open('Item deleted', 'Close', { duration: 3000 });
        this.loadLookups();
      }
    });
  }

  deleteLookup(lookup: Lookup): void {
    if (confirm(`Delete lookup "${lookup.name}"?`)) {
      this.templateService.deleteLookup(lookup.id).subscribe(res => {
        if (res.success) {
          this.snackBar.open('Lookup deleted', 'Close', { duration: 3000 });
          this.loadLookups();
        }
      });
    }
  }
}