import { Component, OnInit, OnDestroy, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TemplateService } from '../../services/template.service';
import {
  DesignerDocument, DesignerElement, DesignerElementType, DesignerFieldType,
  ElementStyles, createDefaultDesign, createElement
} from '../../models/designer.models';

@Component({
  selector: 'app-template-designer',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="designer-container">
      <!-- Toolbar -->
      <div class="toolbar">
        <div class="toolbar-left">
          <button class="btn btn-sm" (click)="saveDesign()" [disabled]="saving()">
            {{ saving() ? 'Saving...' : '💾 Save' }}
          </button>
          <button class="btn btn-sm" (click)="previewTemplate()">👁 Preview</button>
          <button class="btn btn-sm" (click)="printTemplate()">🖨 Print</button>
        </div>
        <div class="toolbar-center">
          <strong>{{ templateName }}</strong>
        </div>
        <div class="toolbar-right">
          <button class="btn btn-sm btn-outline" (click)="undo()">↩ Undo</button>
          <button class="btn btn-sm btn-outline" (click)="deleteSelected()" [disabled]="!selectedElement()">🗑 Delete</button>
        </div>
      </div>

      <div class="designer-body">
        <!-- Toolbox -->
        <div class="toolbox">
          <div class="toolbox-title">Elements</div>
          <div class="toolbox-item" draggable="true" (dragstart)="onDragStart($event, DesignerElementType.Text)">
            <span class="toolbox-icon">T</span> Text
          </div>
          <div class="toolbox-item" draggable="true" (dragstart)="onDragStart($event, DesignerElementType.Header)">
            <span class="toolbox-icon">H</span> Header
          </div>
          <div class="toolbox-item" draggable="true" (dragstart)="onDragStart($event, DesignerElementType.Footer)">
            <span class="toolbox-icon">F</span> Footer
          </div>
          <div class="toolbox-item" draggable="true" (dragstart)="onDragStart($event, DesignerElementType.Field)">
            <span class="toolbox-icon">📝</span> Field
          </div>
          <div class="toolbox-item" draggable="true" (dragstart)="onDragStart($event, DesignerElementType.Signature)">
            <span class="toolbox-icon">✍</span> Signature
          </div>
          <div class="toolbox-item" draggable="true" (dragstart)="onDragStart($event, DesignerElementType.Image)">
            <span class="toolbox-icon">🖼</span> Image
          </div>
          <div class="toolbox-item" draggable="true" (dragstart)="onDragStart($event, DesignerElementType.Table)">
            <span class="toolbox-icon">⊞</span> Table
          </div>
          <div class="toolbox-item" draggable="true" (dragstart)="onDragStart($event, DesignerElementType.Divider)">
            <span class="toolbox-icon">—</span> Divider
          </div>
        </div>

        <!-- Canvas -->
        <div class="canvas-wrapper"
             (dragover)="onDragOver($event)"
             (drop)="onDrop($event)">
          <div class="canvas" 
               [style.width]="design().pageSettings.width"
               [style.min-height]="design().pageSettings.height"
               (click)="selectedElement.set(null)">
            @for (element of design().elements; track element.id) {
              <div class="canvas-element"
                   [class.selected]="selectedElement()?.id === element.id"
                   [style.left.px]="element.x"
                   [style.top.px]="element.y"
                   [style.width.px]="element.width"
                   [style.height.px]="element.height"
                   [style.font-size.px]="element.styles.fontSize"
                   [style.font-weight]="element.styles.bold ? 'bold' : 'normal'"
                   [style.font-style]="element.styles.italic ? 'italic' : 'normal'"
                   [style.color]="element.styles.color"
                   [style.background-color]="element.styles.backgroundColor"
                   [style.text-align]="element.styles.textAlign"
                   [style.padding.px]="element.styles.padding"
                   [style.border-top]="element.styles.borderTop"
                   [style.border-bottom]="element.styles.borderBottom"
                   [style.border-left]="element.styles.borderLeft"
                   [style.border-right]="element.styles.borderRight"
                   (mousedown)="onElementMouseDown($event, element)"
                   (click)="selectElement($event, element)">
                @switch (element.type) {
                  @case (DesignerElementType.Text) {
                    <div class="element-content" [innerText]="element.content"></div>
                  }
                  @case (DesignerElementType.Header) {
                    <div class="element-content" [innerText]="element.content"></div>
                  }
                  @case (DesignerElementType.Footer) {
                    <div class="element-content" [innerText]="element.content"></div>
                  }
                  @case (DesignerElementType.Field) {
                    <div class="element-field">
                      @if (element.label) {
                        <div class="field-label">{{ element.label }}</div>
                      }
                      <div class="field-input" [style.border-bottom]="'1px solid #999'">
                        {{ element.placeholder || '...' }}
                      </div>
                    </div>
                  }
                  @case (DesignerElementType.Signature) {
                    <div class="element-signature">
                      <div class="signature-line"></div>
                      @if (element.label) {
                        <div class="signature-label">{{ element.label }}</div>
                      }
                    </div>
                  }
                  @case (DesignerElementType.Image) {
                    <div class="element-image">
                      @if (element.imageSrc) {
                        <img [src]="element.imageSrc" style="width:100%;height:100%;object-fit:contain" />
                      } @else {
                        <div class="image-placeholder">[Image]</div>
                      }
                    </div>
                  }
                  @case (DesignerElementType.Table) {
                    <table class="element-table">
                      <thead>
                        <tr>
                          @for (col of element.tableColumns; track col) {
                            <th>{{ col }}</th>
                          }
                        </tr>
                      </thead>
                    </table>
                  }
                  @case (DesignerElementType.Divider) {
                    <hr style="border:none;border-top:1px solid #000;margin:0;" />
                  }
                }
                <!-- Resize handles -->
                <div class="resize-handle top-left"></div>
                <div class="resize-handle top-right"></div>
                <div class="resize-handle bottom-left"></div>
                <div class="resize-handle bottom-right"></div>
              </div>
            }
          </div>
        </div>

        <!-- Properties Panel -->
        <div class="properties-panel">
          @if (selectedElement(); as el) {
            <div class="properties-header">Properties</div>
            
            <div class="prop-group">
              <label class="prop-label">Type</label>
              <div class="prop-value badge">{{ el.type }}</div>
            </div>
            
            @if (el.type === DesignerElementType.Text || el.type === DesignerElementType.Header || el.type === DesignerElementType.Footer) {
              <div class="prop-group">
                <label class="prop-label">Content</label>
                <textarea class="prop-input" [value]="el.content" (input)="updateProp('content', $any($event.target).value)" rows="2"></textarea>
              </div>
            }
            
            @if (el.type === DesignerElementType.Field) {
              <div class="prop-group">
                <label class="prop-label">Label</label>
                <input class="prop-input" [value]="el.label" (input)="updateProp('label', $any($event.target).value)" />
              </div>
              <div class="prop-group">
                <label class="prop-label">Field Type</label>
                <select class="prop-input" [value]="el.fieldType" (change)="updateProp('fieldType', $any($event.target).value)">
                  <option value="Text">Text</option>
                  <option value="LongText">Long Text</option>
                  <option value="Number">Number</option>
                  <option value="Date">Date</option>
                  <option value="Dropdown">Dropdown</option>
                  <option value="Checkbox">Checkbox</option>
                </select>
              </div>
              <div class="prop-group">
                <label class="prop-label">Placeholder</label>
                <input class="prop-input" [value]="el.placeholder" (input)="updateProp('placeholder', $any($event.target).value)" />
              </div>
              <div class="prop-group inline">
                <label class="prop-label">Required</label>
                <input type="checkbox" [checked]="el.required" (change)="updateProp('required', $any($event.target).checked)" />
              </div>
            }
            
            @if (el.type === DesignerElementType.Signature) {
              <div class="prop-group">
                <label class="prop-label">Label</label>
                <input class="prop-input" [value]="el.label" (input)="updateProp('label', $any($event.target).value)" />
              </div>
            }
            
            @if (el.type === DesignerElementType.Image) {
              <div class="prop-group">
                <label class="prop-label">Image URL</label>
                <input class="prop-input" [value]="el.imageSrc" (input)="updateProp('imageSrc', $any($event.target).value)" placeholder="assets:guid or URL" />
              </div>
            }
            
            @if (el.type === DesignerElementType.Table) {
              <div class="prop-group">
                <label class="prop-label">Columns (comma separated)</label>
                <input class="prop-input" [value]="el.tableColumns?.join(', ')" (input)="updateTableColumns($any($event.target).value)" />
              </div>
            }
            
            <hr class="prop-divider" />
            <div class="prop-section-title">Position & Size</div>
            
            <div class="prop-row">
              <div class="prop-group half">
                <label class="prop-label">X</label>
                <input class="prop-input" type="number" [value]="el.x" (input)="updatePropNumber('x', $any($event.target).value)" />
              </div>
              <div class="prop-group half">
                <label class="prop-label">Y</label>
                <input class="prop-input" type="number" [value]="el.y" (input)="updatePropNumber('y', $any($event.target).value)" />
              </div>
            </div>
            <div class="prop-row">
              <div class="prop-group half">
                <label class="prop-label">Width</label>
                <input class="prop-input" type="number" [value]="el.width" (input)="updatePropNumber('width', $any($event.target).value)" />
              </div>
              <div class="prop-group half">
                <label class="prop-label">Height</label>
                <input class="prop-input" type="number" [value]="el.height" (input)="updatePropNumber('height', $any($event.target).value)" />
              </div>
            </div>
            
            <hr class="prop-divider" />
            <div class="prop-section-title">Styling</div>
            
            <div class="prop-row">
              <div class="prop-group half">
                <label class="prop-label">Font Size</label>
                <input class="prop-input" type="number" [value]="el.styles.fontSize" (input)="updateStyleNumber('fontSize', $any($event.target).value)" />
              </div>
              <div class="prop-group half">
                <label class="prop-label">Color</label>
                <input class="prop-input color-input" type="color" [value]="el.styles.color" (input)="updateStyle('color', $any($event.target).value)" />
              </div>
            </div>
            <div class="prop-row">
              <div class="prop-group half">
                <label class="prop-label">Bg Color</label>
                <input class="prop-input color-input" type="color" [value]="el.styles.backgroundColor || '#ffffff'" (input)="updateStyle('backgroundColor', $any($event.target).value)" />
              </div>
              <div class="prop-group half">
                <label class="prop-label">Align</label>
                <select class="prop-input" [value]="el.styles.textAlign" (change)="updateStyle('textAlign', $any($event.target).value)">
                  <option value="left">Left</option>
                  <option value="center">Center</option>
                  <option value="right">Right</option>
                </select>
              </div>
            </div>
            <div class="prop-row inline-group">
              <label class="prop-label inline"><input type="checkbox" [checked]="el.styles.bold" (change)="updateStyle('bold', $any($event.target).checked)" /> Bold</label>
              <label class="prop-label inline"><input type="checkbox" [checked]="el.styles.italic" (change)="updateStyle('italic', $any($event.target).checked)" /> Italic</label>
            </div>
              <div class="prop-group">
                <label class="prop-label">Padding</label>
                <input class="prop-input" type="number" [value]="el.styles.padding" (input)="updateStyleNumber('padding', $any($event.target).value)" />
              </div>
          } @else {
            <div class="properties-header">Properties</div>
            <div class="no-selection">Select an element on the canvas to edit its properties</div>
          }
        </div>
      </div>
    </div>
  `,
  styles: [`
    .designer-container { display: flex; flex-direction: column; height: calc(100vh - 64px); background: #f5f5f5; }
    .toolbar { display: flex; align-items: center; justify-content: space-between; padding: 8px 16px; background: #fff; border-bottom: 1px solid #ddd; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }
    .toolbar-left, .toolbar-right { display: flex; gap: 6px; }
    .toolbar-center { font-size: 16px; font-weight: 600; color: #333; }
    .btn { padding: 6px 14px; border: 1px solid #ccc; border-radius: 4px; background: #fff; cursor: pointer; font-size: 13px; }
    .btn:disabled { opacity: 0.5; cursor: default; }
    .btn:hover:not(:disabled) { background: #f0f0f0; }
    .btn-outline { background: transparent; border: 1px solid #aaa; }
    .designer-body { display: flex; flex: 1; overflow: hidden; }
    
    .toolbox { width: 100px; background: #fff; border-right: 1px solid #ddd; padding: 8px; overflow-y: auto; }
    .toolbox-title { font-size: 12px; font-weight: 600; text-transform: uppercase; color: #666; margin-bottom: 8px; }
    .toolbox-item { padding: 8px; margin-bottom: 4px; border-radius: 4px; cursor: grab; font-size: 13px; background: #fafafa; border: 1px solid #eee; user-select: none; }
    .toolbox-item:hover { background: #e8f4fd; border-color: #90caf9; }
    .toolbox-icon { display: inline-block; width: 20px; text-align: center; margin-right: 4px; }
    
    .canvas-wrapper { flex: 1; overflow: auto; padding: 20px; background: #e8e8e8; display: flex; justify-content: center; }
    .canvas { background: #fff; box-shadow: 0 2px 8px rgba(0,0,0,0.15); position: relative; margin: 0 auto; }
    
    .canvas-element { position: absolute; cursor: move; border: 1px solid transparent; box-sizing: border-box; overflow: hidden; }
    .canvas-element:hover { border-color: #90caf9; }
    .canvas-element.selected { border-color: #1976d2; background: rgba(25,118,210,0.04); }
    .canvas-element.selected .resize-handle { display: block; }
    
    .resize-handle { display: none; position: absolute; width: 8px; height: 8px; background: #1976d2; border: 1px solid #fff; border-radius: 50%; }
    .resize-handle.top-left { top: -4px; left: -4px; cursor: nw-resize; }
    .resize-handle.top-right { top: -4px; right: -4px; cursor: ne-resize; }
    .resize-handle.bottom-left { bottom: -4px; left: -4px; cursor: sw-resize; }
    .resize-handle.bottom-right { bottom: -4px; right: -4px; cursor: se-resize; }
    
    .element-content { width: 100%; height: 100%; overflow: hidden; }
    .field-label { font-size: 12px; color: #666; margin-bottom: 2px; }
    .field-input { min-height: 24px; color: #999; font-style: italic; }
    .signature-line { border-top: 1px solid #000; width: 150px; margin: 8px auto; }
    .signature-label { font-size: 12px; text-align: center; color: #666; }
    .image-placeholder { background: #eee; width: 100%; height: 100%; display: flex; align-items: center; justify-content: center; color: #999; }
    .element-table { width: 100%; border-collapse: collapse; }
    .element-table th { border: 1px solid #000; padding: 4px; font-size: 12px; background: #f5f5f5; }
    
    .properties-panel { width: 240px; background: #fff; border-left: 1px solid #ddd; padding: 12px; overflow-y: auto; }
    .properties-header { font-size: 14px; font-weight: 600; color: #333; margin-bottom: 12px; padding-bottom: 8px; border-bottom: 1px solid #eee; }
    .no-selection { font-size: 13px; color: #999; padding: 20px 0; }
    .badge { display: inline-block; padding: 2px 8px; border-radius: 3px; background: #e3f2fd; color: #1565c0; font-size: 11px; font-weight: 600; }
    .prop-group { margin-bottom: 8px; }
    .prop-group.inline { display: flex; align-items: center; gap: 8px; }
    .prop-label { display: block; font-size: 11px; color: #666; margin-bottom: 2px; font-weight: 500; }
    .prop-label.inline { display: inline-flex; align-items: center; gap: 4px; margin-right: 12px; }
    .prop-input { width: 100%; padding: 4px 6px; border: 1px solid #ddd; border-radius: 3px; font-size: 12px; box-sizing: border-box; }
    .prop-input.color-input { height: 30px; padding: 2px; }
    .prop-input:focus { outline: none; border-color: #1976d2; }
    .prop-row { display: flex; gap: 8px; }
    .prop-group.half { flex: 1; }
    .inline-group { display: flex; gap: 8px; margin-bottom: 8px; }
    .prop-divider { border: none; border-top: 1px solid #eee; margin: 12px 0; }
    .prop-section-title { font-size: 12px; font-weight: 600; color: #555; margin-bottom: 8px; }
    textarea.prop-input { resize: vertical; min-height: 40px; }
    select.prop-input { height: 26px; }
  `]
})
export class TemplateDesignerComponent implements OnInit, OnDestroy {
  readonly DesignerElementType = DesignerElementType;
  readonly Number = Number;

  templateId: string | null = null;
  templateName = 'New Template';
  
  design = signal<DesignerDocument>(createDefaultDesign());
  selectedElement = signal<DesignerElement | null>(null);
  saving = signal(false);
  
  private history: DesignerDocument[] = [];
  private historyIndex = -1;
  private isDragging = false;
  private dragElement: DesignerElement | null = null;
  private dragOffsetX = 0;
  private dragOffsetY = 0;
  private resizeDir: string | null = null;

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
    document.addEventListener('mousemove', this.onMouseMove);
    document.addEventListener('mouseup', this.onMouseUp);
  }

  ngOnDestroy() {
    document.removeEventListener('mousemove', this.onMouseMove);
    document.removeEventListener('mouseup', this.onMouseUp);
  }

  private loadTemplate(id: string) {
    this.templateService.getTemplate(id).subscribe(res => {
      if (res.data) {
        this.templateName = res.data.name;
        try {
          const doc = JSON.parse(res.data.designJson || '{}') as DesignerDocument;
          this.design.set(doc);
          this.pushHistory();
        } catch {
          this.design.set(createDefaultDesign());
          this.pushHistory();
        }
      }
    });
  }

  onDragStart(event: DragEvent, type: DesignerElementType) {
    event.dataTransfer?.setData('text/plain', type);
    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = 'copy';
    }
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    if (event.dataTransfer) {
      event.dataTransfer.dropEffect = 'copy';
    }
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    const typeStr = event.dataTransfer?.getData('text/plain');
    if (!typeStr) return;
    
    const type = typeStr as DesignerElementType;
    const canvas = event.currentTarget as HTMLElement;
    const canvasRect = canvas.getBoundingClientRect();
    const x = event.clientX - canvasRect.left - 100;
    const y = event.clientY - canvasRect.top - 25;

    const element = createElement(type as DesignerElementType);
    element.x = Math.max(0, Math.round(x / 10) * 10);
    element.y = Math.max(0, Math.round(y / 10) * 10);
    
    this.design.update(d => ({ ...d, elements: [...d.elements, element] }));
    this.selectedElement.set(element);
    this.pushHistory();
  }

  selectElement(event: MouseEvent, element: DesignerElement) {
    event.stopPropagation();
    this.selectedElement.set(element);
  }

  onElementMouseDown(event: MouseEvent, element: DesignerElement) {
    if ((event.target as HTMLElement).classList.contains('resize-handle')) {
      this.resizeDir = (event.target as HTMLElement).className.includes('top-left') ? 'tl' :
                       (event.target as HTMLElement).className.includes('top-right') ? 'tr' :
                       (event.target as HTMLElement).className.includes('bottom-left') ? 'bl' : 'br';
      this.dragElement = element;
      this.dragOffsetX = event.clientX;
      this.dragOffsetY = event.clientY;
      return;
    }
    
    this.isDragging = true;
    this.dragElement = element;
    this.dragOffsetX = event.clientX - element.x;
    this.dragOffsetY = event.clientY - element.y;
    this.selectedElement.set(element);
  }

  private onMouseMove = (event: MouseEvent) => {
    if (!this.dragElement) return;
    
    if (this.resizeDir) {
      const dx = event.clientX - this.dragOffsetX;
      const dy = event.clientY - this.dragOffsetY;
      
      this.design.update(d => ({
        ...d,
        elements: d.elements.map(el => {
          if (el.id !== this.dragElement!.id) return el;
          const updated = { ...el };
          if (this.resizeDir!.includes('r')) updated.width = Math.max(30, el.width + dx);
          if (this.resizeDir!.includes('l')) { updated.x = el.x + dx; updated.width = Math.max(30, el.width - dx); }
          if (this.resizeDir!.includes('b')) updated.height = Math.max(10, el.height + dy);
          if (this.resizeDir!.includes('t')) { updated.y = el.y + dy; updated.height = Math.max(10, el.height - dy); }
          return updated;
        })
      }));
      
      this.dragOffsetX = event.clientX;
      this.dragOffsetY = event.clientY;
      return;
    }
    
    if (!this.isDragging) return;
    
    const newX = Math.max(0, event.clientX - this.dragOffsetX);
    const newY = Math.max(0, event.clientY - this.dragOffsetY);
    
    this.design.update(d => ({
      ...d,
      elements: d.elements.map(el => 
        el.id === this.dragElement!.id 
          ? { ...el, x: Math.round(newX / 5) * 5, y: Math.round(newY / 5) * 5 }
          : el
      )
    }));
  };

  private onMouseUp = () => {
    if (this.isDragging || this.resizeDir) {
      this.pushHistory();
    }
    this.isDragging = false;
    this.dragElement = null;
    this.resizeDir = null;
  };

  updateProp(prop: string, value: any) {
    this.selectedElement.update(el => {
      if (!el) return null;
      return { ...el, [prop]: value };
    });
    this.design.update(d => ({
      ...d,
      elements: d.elements.map(el => 
        el.id === this.selectedElement()?.id ? this.selectedElement()! : el
      )
    }));
  }

  updateStyle(prop: string, value: any) {
    this.selectedElement.update(el => {
      if (!el) return null;
      return { ...el, styles: { ...el.styles, [prop]: value } };
    });
    this.design.update(d => ({
      ...d,
      elements: d.elements.map(el => 
        el.id === this.selectedElement()?.id ? this.selectedElement()! : el
      )
    }));
  }

  updateTableColumns(value: string) {
    const cols = value.split(',').map(c => c.trim()).filter(c => c.length > 0);
    this.updateProp('tableColumns', cols);
  }

  updatePropNumber(prop: string, value: string) {
    this.updateProp(prop, parseInt(value, 10) || 0);
  }

  updateStyleNumber(prop: string, value: string) {
    this.updateStyle(prop, parseInt(value, 10) || 0);
  }

  deleteSelected() {
    const el = this.selectedElement();
    if (!el) return;
    this.design.update(d => ({
      ...d,
      elements: d.elements.filter(e => e.id !== el.id)
    }));
    this.selectedElement.set(null);
    this.pushHistory();
  }

  saveDesign() {
    this.saving.set(true);
    const json = JSON.stringify(this.design());
    
    if (this.templateId) {
      this.templateService.updateDesign(this.templateId, json).subscribe({
        next: () => this.saving.set(false),
        error: () => this.saving.set(false)
      });
    }
  }

  previewTemplate() {
    this.saveDesign();
    window.open(`/preview/${this.templateId}`, '_blank');
  }

  printTemplate() {
    const json = JSON.stringify(this.design());
    const w = window.open('', '_blank');
    if (w) {
      w.document.write(this.buildPreviewHtml());
      w.document.close();
    }
  }

  private buildPreviewHtml(): string {
    const doc = this.design();
    const page = doc.pageSettings;
    const elements = doc.elements.map(el => {
      const s = el.styles;
      const pos = `position:absolute;left:${el.x}px;top:${el.y}px;width:${el.width}px;height:${el.height}px;`;
      const css = `font-size:${s.fontSize}px;color:${s.color};text-align:${s.textAlign};padding:${s.padding}px;${s.bold?'font-weight:bold;':''}${s.italic?'font-style:italic;':''}${s.backgroundColor?'background-color:'+s.backgroundColor+';':''}`;
      
      let inner = '';
      switch (el.type) {
        case 'Text': case 'Header': case 'Footer': inner = el.content || ''; break;
        case 'Field': inner = `<div>${el.label ? `<div style="font-size:12px;color:#666">${el.label}</div>`:''}<span style="border-bottom:1px solid #999;display:inline-block;min-width:100px">&nbsp;</span></div>`; break;
        case 'Signature': inner = `<div style="border-top:1px solid #000;width:150px;margin-top:15px"></div>${el.label?`<div style="font-size:11px;text-align:center">${el.label}</div>`:''}`; break;
        case 'Image': inner = el.imageSrc ? `<img src="${el.imageSrc}" style="width:100%;height:100%;object-fit:contain"/>` : '<div style="background:#eee;height:100%"></div>'; break;
        case 'Table': inner = `<table style="width:100%;border-collapse:collapse">${(el.tableColumns||[]).map(c=>`<th style="border:1px solid #000;padding:4px;font-size:12px">${c}</th>`).join('')}</table>`; break;
        case 'Divider': inner = '<hr style="border:none;border-top:1px solid #000;margin:0">'; break;
      }
      return `<div style="${pos}${css}">${inner}</div>`;
    }).join('\n');

    return `<!DOCTYPE html><html><head><style>
      @page { size: ${page.width} ${page.height}; margin: ${page.marginTop} ${page.marginRight} ${page.marginBottom} ${page.marginLeft}; }
      body { position:relative; width:100%; height:100%; font-family:Arial,sans-serif; margin:0; padding:0; }
      @media print { body { margin: 0; } }
    </style></head><body>${elements}</body></html>`;
  }

  undo() {
    if (this.historyIndex > 0) {
      this.historyIndex--;
      this.design.set(JSON.parse(JSON.stringify(this.history[this.historyIndex])));
    }
  }

  private pushHistory() {
    this.historyIndex++;
    this.history = this.history.slice(0, this.historyIndex);
    this.history.push(JSON.parse(JSON.stringify(this.design())));
    if (this.history.length > 50) this.history.shift();
  }
}