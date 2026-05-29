import { FasonMutabakComponent } from './components/userPages/fason-mutabak/fason-mutabak.component';
import { FasonHakedisFiyatlariComponent } from './components/userPages/fason-hakedis-fiyatlari/fason-hakedis-fiyatlari.component';
import { FasonHakedisFasonComponent } from './components/userPages/fason-hakedis-fason/fason-hakedis-fason.component';
import { FasonHakedisFasonFiyatliComponent } from './components/userPages/fason-hakedis-fason-fiyatli/fason-hakedis-fason-fiyatli.component';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { HomeComponent } from './components/userPages/home/home.component';
import { FasonOzetComponent } from './components/userPages/fason-ozet/fason-ozet.component';
import { AuthGuard } from './guards/auth.guard';
import { LoginRedirectGuard } from './guards/login-redirect.guard';
import { ShellComponent } from './layout/shell.component';
import { FasonAylikHareketlerComponent } from './components/userPages/fason-aylik-hareketler/fason-aylik-hareketler.component';
import { FasonIslemComponent } from './components/userPages/fason-islem/fason-islem.component';
import { FasonHakedisUrunMiktarliComponent } from './components/userPages/fason-hakedis-urun-miktarli/fason-hakedis-urun-miktarli.component';
import { FasonHakedisKoliliComponent } from './components/userPages/fason-hakedis-kolili/fason-hakedis-kolili.component';

const routes: Routes = [
  { path: 'login', component: LoginComponent, canActivate: [LoginRedirectGuard] },

  {
    path: '',
    component: ShellComponent,
    canActivate: [AuthGuard],
    canActivateChild: [AuthGuard],
    children: [
  { path: 'dashboard', component: HomeComponent, data: { roles: ['User'] } },
  { path: 'fason-hakedis', component: require('./components/userPages/fason-hakedis/fason-hakedis.component').FasonHakedisComponent, data: { roles: ['User'] } },
  { path: 'fason-ozet', component: FasonOzetComponent, data: { roles: ['User'] } },
  { path: 'fason-aylik-hareketler', component: FasonAylikHareketlerComponent, data: { roles: ['User'] } },
  { path: 'fason-islem', component: FasonIslemComponent, data: { roles: ['User'] } },
  { path: 'fason-hakedis-urun-miktarli', component: FasonHakedisUrunMiktarliComponent, data: { roles: ['User'] } },
  { path: 'fason-hakedis-kolili', component: FasonHakedisKoliliComponent, data: { roles: ['User'] } },
  { path: 'fason-hakedis-fason', component: FasonHakedisFasonComponent, data: { roles: ['User'] } },
  { path: 'fason-hakedis-fiyatlari', component: FasonHakedisFiyatlariComponent, data: { roles: ['User'] } },
  { path: 'fason-mutabak', component: FasonMutabakComponent, data: { roles: ['User'] } },
  { path: 'fason-hakedis-fason-fiyatli', component: FasonHakedisFasonFiyatliComponent, data: { roles: ['User'] } },
  ]
  },
    { path: '', pathMatch: 'full', redirectTo: 'login' },

  { path: '**', redirectTo: 'login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { bindToComponentInputs: true })],
  exports: [RouterModule]
})
export class AppRoutingModule {}
