import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TemplateService } from '../../services/template.service';
import {
  DesignerDocument, DesignerElement, DesignerElementType, DesignerFieldType
} from '../../models/designer.models';

@Component({
  selector: 'app-template-fill',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="fill-container">
      <div class="fill-header">
        <h2>{{ templateName }}</h2>
        <div>
          <button class="btn btn-primary" (click)="renderAndPrint()">🖨 Print</button>
          <button class="btn" (click)="goBack()">← Back</button>
        </div>
      </div>
      
      <div class="fill-body">
        <div class="fill-form">
          <h3>Fill in the fields</h3>
          @if (fields().length === 0) {
            <p class="no-fields">This template has no fillable fields.</p>
          }
          @for (field of fields(); track field.id) {
            <div class="form-field">
              <label>
                {{ field.label || field.id }}
                @if (field.required) { <span class="required">*</span> }
              </label>
              
              @switch (field.fieldType) {
                @case (DesignerFieldType.LongText) {
                  <textarea 
                    [placeholder]="field.placeholder || 'Enter text...'"
                    [(ngModel)]="fieldValues[field.id]"
                    rows="3">
                  </textarea>
                }
                @case (DesignerFieldType.Number) {
                  <input type="number" 
                    [placeholder]="field.placeholder || 'Enter number...'"
                    [(ngModel)]="fieldValues[field.id]" />
                }
                @case (DesignerFieldType.Date) {
                  <input type="date" [(ngModel)]="fieldValues[field.id]" />
                }
                @case (DesignerFieldType.Checkbox) {
                  <input type="checkbox" 
                    [checked]="fieldValues[field.id] === 'true'"
                    (change)="fieldValues[field.id] = $any($event.target).checked ? 'true' : 'false'" />
                }
                @case (DesignerFieldType.Dropdown) {
                  <select [(ngModel)]="fieldValues[field.id]">
                    <option value="">-- Select --</option>
                    <option value="Option 1">Option 1</option>
                    <option value="Option 2">Option 2</option>
                    <option value="Option 3">Option 3</option>
                  </select>
                }
                @default {
                  <input type="text" 
                    [placeholder]="field.placeholder || 'Enter text...'"
                    [(ngModel)]="fieldValues[field.id]" />
                }
              }
            </div>
          }
        </div>
      </div>
    </div>
  `,
  styles: [`
    .fill-container { padding: 20px; max-width: 800px; margin: 0 auto; }
    .fill-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
    .fill-header h2 { margin: 0; font-size: 24px; color: #333; }
    .fill-body { background: #fff; border-radius: 8px; box-shadow: 0 1px 4px rgba(0,0,0,0.1); padding: 24px; }
    .fill-form h3 { margin-top: 0; margin-bottom: 16px; font-size: 18px; color: #333; border-bottom: 1px solid #eee; padding-bottom: 8px; }
    .no-fields { color: #999; }
    .form-field { margin-bottom: 16px; }
    .form-field label { display: block; font-weight: 500; margin-bottom: 4px; color: #333; font-size: 14px; }
    .required { color: #d32f2f; }
    .form-field input[type="text"], .form-field input[type="number"], .form-field input[type="date"], 
    .form-field textarea, .form-field select { width: 100%; padding: 8px 12px; border: 1px solid #ddd; border-radius: 4px; font-size: 14px; box-sizing: border-box; }
    .form-field input[type="text"]:focus, .form-field textarea:focus, .form-field select:focus { outline: none; border-color: #1976d2; }
    .form-field input[type="checkbox"] { width: 20px; height: 20px; }
    .btn { padding: 8px 16px; border: 1px solid #ccc; border-radius: 4px; background: #fff; cursor: pointer; font-size: 14px; margin-left: 8px; }
    .btn-primary { background: #1976d2; color: #fff; border-color: #1976d2; }
    .btn-primary:hover { background: #1565c0; }
  `]
})
export class TemplateFillComponent implements OnInit {
  readonly DesignerFieldType = DesignerFieldType;
  
  templateId: string | null = null;
  templateName = '';
  fields = signal<DesignerElement[]>([]);
  fieldValues: Record<string, string> = {};

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private templateService: TemplateService
  ) {}

  ngOnInit() {
    this.templateId = this.route.snapshot.paramMap.get('id');
    if (this.templateId) {
      this.loadTemplate(this.templateId);
    }
  }

  private loadTemplate(id: string) {
    this.templateService.getTemplate(id).subscribe(res => {
      if (res.data) {
        this.templateName = res.data.name;
        try {
          const doc = JSON.parse(res.data.designJson || '{}') as DesignerDocument;
          this.fields.set(doc.elements.filter(e => e.type === DesignerElementType.Field));
          this.fields().forEach(f => { this.fieldValues[f.id] = ''; });
        } catch {}
      }
    });
  }

  renderAndPrint() {
    if (!this.templateId) return;
    this.templateService.renderTemplate(this.templateId, this.fieldValues).subscribe(html => {
      const w = window.open('', '_blank');
      if (w) {
        w.document.write(html);
        w.document.close();
        w.print();
      }
    });
  }

  goBack() {
    this.router.navigate(['/templates']);
  }
}