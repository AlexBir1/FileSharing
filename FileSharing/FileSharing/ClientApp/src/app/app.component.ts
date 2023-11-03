import { Component, OnInit } from '@angular/core';
import * as $ from 'jquery';

import { AccountService } from './services/account.service';
import { FilesService } from './services/files.service';
import { SettingsService } from './services/settings.service';


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
  title = 'app';
  constructor(private accountService: AccountService, private fileService: FilesService, private settingsService: SettingsService) { }
  ngOnInit(): void {
    this.settingsService.equalizeSettingsWithDB().subscribe(x => {
      if (x.isSuccessful) {
        this.accountService.refreshAccount();
        this.settingsService.refreshSettings();
        this.fileService.equalizeFileServerWithDB().subscribe(x => console.log(x));
        this.setSiteTheme();
      }
    });
  }

  setSiteTheme() {
    this.settingsService.currentSettings$.subscribe(x => {
      if (x?.find(x => x.key === 'isDarkTheme')?.value) {
        $('body').removeAttr('data-bs-theme');
        $('body').attr('data-bs-theme', 'dark');
      }
      else {
        $('body').removeAttr('data-bs-theme');
        $('body').attr('data-bs-theme', 'light');
      }
    });
  }
}
