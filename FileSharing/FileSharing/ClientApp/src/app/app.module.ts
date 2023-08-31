import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { TooltipModule } from 'ngx-bootstrap/tooltip';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { AuthComponent } from './auth/auth.component';
import { FilesComponent } from './files/files.component';
import { AccountComponent } from './account/account.component';
import { AccountService } from './account.service';
import { FilesService } from './files.service';
import { NgxPaginationModule } from 'ngx-pagination';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations'
import { BsModalService, ModalModule } from 'ngx-bootstrap/modal';
import { CategoryComponent } from './category/category.component';
import { CategoryService } from './category.service';
import { SettingsComponent } from './settings/settings.component';
import { SettingsService } from './settings.service';
import { ProgressbarModule } from 'ngx-bootstrap/progressbar';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    AuthComponent,
    FilesComponent,
    AccountComponent,
    CategoryComponent,
    SettingsComponent,
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    NgxPaginationModule,
    TooltipModule,
    ProgressbarModule.forRoot(),
    ModalModule.forRoot(),
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'auth', component: AuthComponent },
      { path: 'files', component: FilesComponent },
      { path: 'account', component: AccountComponent },
      { path: 'category', component: CategoryComponent },
      { path: 'settings', component: SettingsComponent },
    ]),
    BrowserAnimationsModule
  ],
  providers: [AccountService, FilesService, TooltipModule, BsModalService, CategoryService, SettingsService],
  bootstrap: [AppComponent]
})
export class AppModule { }
