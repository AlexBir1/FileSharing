import { Component, OnInit } from '@angular/core';
import { FileInfoModel } from '../Entity/FileInfoModel';

import { FilesService } from '../services/files.service';
import { SettingsService } from '../services/settings.service';


@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent implements OnInit {
  randomFiles: FileInfoModel[] = [];
  lastUploadedFiles: FileInfoModel[] = [];
  mostDownloadedFiles: FileInfoModel[] = [];

  constructor(public filesService: FilesService, private settingsService: SettingsService)
  {

  }

  onDownloadSubmit(file: FileInfoModel) {
    return this.filesService.downloadFile(file);
  }

  ngOnInit(): void {
    this.initFileArrays();
    this.settingsService.currentSettings$.subscribe(x => {
      if (x?.find(y => y.key === 'showRandomFiles')?.value  === true) {
        this.filesService.getRandomFiles(3).subscribe(x => {
          if (x.isSuccessful) {
            this.randomFiles = x.data;
          }
        });
      }
      if (x?.find(y => y.key === 'showLastUploadedFiles')?.value === true) {
        this.filesService.getLastUploadedFiles(3).subscribe(x => {
          if (x.isSuccessful) {
            this.lastUploadedFiles = x.data;
          }
        });
      }
      if (x?.find(y => y.key === 'showMostDownloadedFiles')?.value === true) {
        this.filesService.getMostDownloadedFiles(3).subscribe(x => {
          if (x.isSuccessful) {
            this.mostDownloadedFiles = x.data;
          }
        });
      } 
    });
  }

  initFileArrays() {
    this.randomFiles = [];
    this.lastUploadedFiles = [];
    this.mostDownloadedFiles = [];
  }
}
