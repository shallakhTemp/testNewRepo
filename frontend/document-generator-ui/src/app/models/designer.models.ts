export enum DesignerElementType {
  Text = 'Text',
  Image = 'Image',
  Field = 'Field',
  Signature = 'Signature',
  Table = 'Table',
  Divider = 'Divider',
  Header = 'Header',
  Footer = 'Footer'
}

export enum DesignerFieldType {
  Text = 'Text',
  LongText = 'LongText',
  Number = 'Number',
  Date = 'Date',
  Dropdown = 'Dropdown',
  Checkbox = 'Checkbox'
}

export interface PageSettings {
  width: string;
  height: string;
  marginTop: string;
  marginBottom: string;
  marginLeft: string;
  marginRight: string;
}

export interface ElementStyles {
  fontSize: number;
  bold: boolean;
  italic: boolean;
  fontFamily?: string;
  color: string;
  backgroundColor?: string;
  textAlign: string;
  borderTop?: string;
  borderBottom?: string;
  borderLeft?: string;
  borderRight?: string;
  padding: number;
}

export interface DesignerElement {
  id: string;
  type: DesignerElementType;
  x: number;
  y: number;
  width: number;
  height: number;
  content?: string;
  label?: string;
  fieldType?: DesignerFieldType;
  required: boolean;
  lookupId?: string;
  placeholder?: string;
  imageSrc?: string;
  tableColumns?: string[];
  styles: ElementStyles;
}

export interface DesignerDocument {
  pageSettings: PageSettings;
  elements: DesignerElement[];
}

export function createDefaultDesign(): DesignerDocument {
  return {
    pageSettings: {
      width: '210mm',
      height: '297mm',
      marginTop: '20mm',
      marginBottom: '20mm',
      marginLeft: '15mm',
      marginRight: '15mm'
    },
    elements: []
  };
}

export function createElement(type: DesignerElementType): DesignerElement {
  const id = crypto.randomUUID();
  switch (type) {
    case DesignerElementType.Text:
      return { id, type, x: 40, y: 40, width: 400, height: 36, content: 'Sample Text', required: false, styles: { fontSize: 16, bold: false, italic: false, color: '#000000', textAlign: 'left', padding: 4 } };
    case DesignerElementType.Header:
      return { id, type, x: 0, y: 0, width: 600, height: 50, content: 'HEADER - Company Name', required: false, styles: { fontSize: 14, bold: true, italic: false, color: '#333333', backgroundColor: '#f0f0f0', textAlign: 'center', padding: 8, borderBottom: '2px solid #000' } };
    case DesignerElementType.Footer:
      return { id, type, x: 0, y: 700, width: 600, height: 40, content: 'Page {page} of {total}', required: false, styles: { fontSize: 12, bold: false, italic: false, color: '#666666', textAlign: 'center', padding: 8, borderTop: '1px solid #ccc' } };
    case DesignerElementType.Field:
      return { id, type, x: 40, y: 100, width: 300, height: 50, label: 'Full Name', fieldType: DesignerFieldType.Text, required: false, placeholder: 'Enter text...', styles: { fontSize: 14, bold: false, italic: false, color: '#000000', textAlign: 'left', padding: 4 } };
    case DesignerElementType.Signature:
      return { id, type, x: 40, y: 200, width: 200, height: 60, label: 'Authorized Signature', required: false, styles: { fontSize: 14, bold: false, italic: false, color: '#000000', textAlign: 'center', padding: 4 } };
    case DesignerElementType.Image:
      return { id, type, x: 40, y: 300, width: 150, height: 150, imageSrc: '', required: false, styles: { fontSize: 14, bold: false, italic: false, color: '#000000', textAlign: 'left', padding: 4 } };
    case DesignerElementType.Table:
      return { id, type, x: 40, y: 480, width: 500, height: 120, tableColumns: ['Column 1', 'Column 2', 'Column 3'], required: false, styles: { fontSize: 12, bold: false, italic: false, color: '#000000', textAlign: 'left', padding: 4 } };
    case DesignerElementType.Divider:
      return { id, type, x: 40, y: 170, width: 500, height: 2, required: false, styles: { fontSize: 1, bold: false, italic: false, color: '#000000', textAlign: 'left', padding: 0, borderTop: '1px solid #000' } };
    default:
      return { id, type, x: 40, y: 40, width: 200, height: 30, content: 'Element', required: false, styles: { fontSize: 14, bold: false, italic: false, color: '#000000', textAlign: 'left', padding: 4 } };
  }
}