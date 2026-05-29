import { FasonMutabakComponent } from './components/userPages/fason-mutabak/fason-mutabak.component';
import { FasonHakedisFiyatlariComponent } from './components/userPages/fason-hakedis-fiyatlari/fason-hakedis-fiyatlari.component';
import { FasonHakedisFasonComponent } from './components/userPages/fason-hakedis-fason/fason-hakedis-fason.component';
import { FasonHakedisFasonFiyatliComponent } from './components/userPages/fason-hakedis-fason-fiyatli/fason-hakedis-fason-fiyatli.component';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';

import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';

import { LoginComponent } from './components/login/login.component';
import { HomeComponent } from './components/userPages/home/home.component';
import { FasonOzetComponent } from './components/userPages/fason-ozet/fason-ozet.component';
import { FasonHakedisComponent } from './components/userPages/fason-hakedis/fason-hakedis.component';
import { FasonIslemModule } from './components/userPages/fason-islem/fason-islem.module';
import { FasonAylikHareketlerModule } from './components/userPages/fason-aylik-hareketler/fason-aylik-hareketler.module';
import { NgChartsModule } from 'ng2-charts';

import { FasonHakedisUrunMiktarliComponent } from './components/userPages/fason-hakedis-urun-miktarli/fason-hakedis-urun-miktarli.component';
import { FasonHakedisKoliliComponent } from './components/userPages/fason-hakedis-kolili/fason-hakedis-kolili.component';

import { TemplateNavbarComponent } from './layout/template-navbar.component';
import { TemplateSidebarComponent } from './layout/template-sidebar.component';
import { ShellComponent } from './layout/shell.component';

import { AuthInterceptor } from './interceptors/auth.interceptor';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    HomeComponent,
  FasonOzetComponent,
  FasonHakedisComponent,
    TemplateNavbarComponent,
    TemplateSidebarComponent,
    ShellComponent
  ,FasonHakedisUrunMiktarliComponent
  ,FasonHakedisKoliliComponent
  ,FasonHakedisFasonComponent
  ,FasonHakedisFiyatlariComponent
  ,FasonMutabakComponent
  ,FasonHakedisFasonFiyatliComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
  AppRoutingModule,
  FasonIslemModule,
  FasonAylikHareketlerModule,
  NgChartsModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {}
