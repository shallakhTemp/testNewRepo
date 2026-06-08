import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from './auth.service';

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface Template {
  id: string;
  code: string;
  name: string;
  description?: string;
  designJson: string;
  isActive: boolean;
  createdOn: string;
  modifiedOn?: string;
}

export interface Lookup {
  id: string;
  name: string;
  description?: string;
  items: LookupItem[];
}

export interface LookupItem {
  id: string;
  lookupId: string;
  value: string;
  text: string;
  displayOrder: number;
}

@Injectable({ providedIn: 'root' })
export class TemplateService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getTemplates(page: number = 1, pageSize: number = 10, search?: string): Observable<ApiResponse<PagedResult<Template>>> {
    let url = `${this.apiUrl}/templates?pageNumber=${page}&pageSize=${pageSize}`;
    if (search) url += `&searchTerm=${encodeURIComponent(search)}`;
    return this.http.get<ApiResponse<PagedResult<Template>>>(url);
  }

  getTemplate(id: string): Observable<ApiResponse<Template>> {
    return this.http.get<ApiResponse<Template>>(`${this.apiUrl}/templates/${id}`);
  }

  createTemplate(template: { code: string; name: string; description?: string; isActive?: boolean; designJson: string }): Observable<ApiResponse<Template>> {
    return this.http.post<ApiResponse<Template>>(`${this.apiUrl}/templates`, template);
  }

  updateTemplate(id: string, template: { code: string; name: string; description?: string; isActive?: boolean; designJson: string }): Observable<ApiResponse<Template>> {
    return this.http.put<ApiResponse<Template>>(`${this.apiUrl}/templates/${id}`, template);
  }

  updateDesign(id: string, designJson: string): Observable<ApiResponse<Template>> {
    return this.http.put<ApiResponse<Template>>(`${this.apiUrl}/templates/${id}/design`, `"${designJson.replace(/"/g, '\\"')}"`);
  }

  deleteTemplate(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/templates/${id}`);
  }

  toggleActive(id: string): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.apiUrl}/templates/${id}/toggle-active`, {});
  }

  renderTemplate(id: string, fieldValues: Record<string, string>): Observable<string> {
    return this.http.post(`${this.apiUrl}/templates/${id}/render`, { templateId: id, fieldValues }, { responseType: 'text' });
  }

  // Lookups
  getLookups(page: number = 1, pageSize: number = 100): Observable<ApiResponse<PagedResult<Lookup>>> {
    return this.http.get<ApiResponse<PagedResult<Lookup>>>(`${this.apiUrl}/lookups?pageNumber=${page}&pageSize=${pageSize}`);
  }

  getLookup(id: string): Observable<ApiResponse<Lookup>> {
    return this.http.get<ApiResponse<Lookup>>(`${this.apiUrl}/lookups/${id}`);
  }

  createLookup(lookup: Partial<Lookup>): Observable<ApiResponse<Lookup>> {
    return this.http.post<ApiResponse<Lookup>>(`${this.apiUrl}/lookups`, lookup);
  }

  updateLookup(id: string, lookup: Partial<Lookup>): Observable<ApiResponse<Lookup>> {
    return this.http.put<ApiResponse<Lookup>>(`${this.apiUrl}/lookups/${id}`, lookup);
  }

  deleteLookup(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/lookups/${id}`);
  }

  createLookupItem(item: Partial<LookupItem>): Observable<ApiResponse<LookupItem>> {
    return this.http.post<ApiResponse<LookupItem>>(`${this.apiUrl}/lookups/items`, item);
  }

  deleteLookupItem(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/lookups/items/${id}`);
  }
}