import { Routes } from '@angular/router';
import { authGuard, adminGuard } from './guards/auth.guard';
import { LayoutComponent } from './components/layout/layout.component';
import { LoginComponent } from './components/login/login.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { TemplatesComponent } from './components/templates/templates.component';
import { TemplateDesignerComponent } from './components/template-designer/template-designer.component';
import { TemplateFillComponent } from './components/template-fill/template-fill.component';
import { LookupsComponent } from './components/lookups/lookups.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', component: DashboardComponent },
      { path: 'templates', component: TemplatesComponent },
      { path: 'templates/design/:id', component: TemplateDesignerComponent, canActivate: [adminGuard] },
      { path: 'templates/fill/:id', component: TemplateFillComponent },
      { path: 'lookups', component: LookupsComponent, canActivate: [adminGuard] },
      { path: '', redirectTo: 'templates', pathMatch: 'full' }
    ]
  },
  { path: '**', redirectTo: 'login' }
];