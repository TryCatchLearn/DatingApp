import { Component, Input } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-confirm-dialog',
  templateUrl: './confirm-dialog.component.html',
  styleUrls: ['./confirm-dialog.component.css']
})
export class ConfirmDialogComponent {
  title = ''
  message = ''
  btnOkText = ''
  btnCancelText = ''
  result = false

  constructor(public modalRef: BsModalRef) {

  }

  confirm() {
    this.result = true
    this.modalRef.hide()
  }

  decline() {
    this.modalRef.hide()
  }
}
