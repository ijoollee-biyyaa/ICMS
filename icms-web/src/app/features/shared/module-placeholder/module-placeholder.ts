import { Component, input } from '@angular/core';
import { MatIcon } from '@angular/material/icon';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-module-placeholder',
  imports: [MatIcon, RouterLink],
  templateUrl: './module-placeholder.html',
  styleUrl: './module-placeholder.scss',
})
export class ModulePlaceholder {
  title = input('');
  icon = input('construction');
  description = input('');
  backLink = input('/district/dashboard', { alias: 'backLink' });
}