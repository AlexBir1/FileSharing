import { Component, OnInit, TemplateRef } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { AccountService } from '../account.service';
import { SettingsModel } from '../Entity/SettingsModel';
import { SettingsService } from '../settings.service';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.css']
})
export class SettingsComponent implements OnInit {

  modalRef?: BsModalRef;
  currentSettings!: SettingsModel[];
  constructor(private accountService: AccountService, private settingsService: SettingsService, private modalService: BsModalService) { }

  ngOnInit() {
    this.accountService.currentAccount$.subscribe(x => {
      this.settingsService.getSettings(x?.id as string).subscribe(x => {
        if (x.isSuccessful) {
          this.currentSettings = x.data;
        }
      });
    });
  }

  applyClick(template: TemplateRef<any>) {
    this.settingsService.setSetting(this.currentSettings).subscribe(x => {
      if (x.isSuccessful) {
        this.settingsService.refreshSettings();
        this.openModal(template);
      }
    });
  }

  openModal(template: TemplateRef<any>) {
    this.modalRef = this.modalService.show(template);
  }
}
