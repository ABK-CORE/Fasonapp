import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FasonIslemComponent } from './fason-islem.component';

@NgModule({
  declarations: [FasonIslemComponent],
  imports: [CommonModule, FormsModule],
  exports: [FasonIslemComponent]
})
export class FasonIslemModule {}
