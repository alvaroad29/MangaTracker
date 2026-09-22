import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MangaListItemDto } from '../../../core/models/lists.models';
import { statusLabels, typeLabels } from '../../../core/models/status-labels';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-list-item-card',
  imports: [RouterLink, DatePipe],
  templateUrl: './list-item-card.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ListItemCardComponent {
  item = input.required<MangaListItemDto>();
  canManage = input(false);

  edit = output<void>();
  remove = output<void>();

  statusLabels = statusLabels;
  typeLabels = typeLabels;
}
