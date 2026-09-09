import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({
  selector: 'app-confirm-modal',
  imports: [],
  templateUrl: './confirm-modal.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmModalComponent {
  open = input.required<boolean>();
  title = input('¿Estás seguro?');
  message = input('');
  confirmLabel = input('Confirmar');
  confirming = input(false);

  confirm = output<void>();
  cancel = output<void>();
}
