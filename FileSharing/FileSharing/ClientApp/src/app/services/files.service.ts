import { Inject, Injectable } from '@angular/core';
import { HttpClient, HttpEventType, HttpRequest, HttpResponse } from '@angular/common/http';

import { ResponseModel } from '../Entity/ResponseModel';
import { FileInfoModel } from '../Entity/FileInfoModel';

import { AccountService } from './account.service';



@Injectable()
export class FilesService {

  private url: string;
  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrl: string, private accountService: AccountService) {
    this.url = baseUrl;
  }

  getFiles() {
    let headers = this.accountService.makeJWTHeader();
    return this.http.get<ResponseModel<FileInfoModel[]>>(this.url + 'files/GetFiles', { headers });
  }

  getMostDownloadedFiles(quantity: number) {
    let headers = this.accountService.makeJWTHeader();
    return this.http.get<ResponseModel<FileInfoModel[]>>(this.url + 'files/GetMostDownloadedFiles/' + quantity, { headers });
  }

  getLastUploadedFiles(quantity: number) {
    let headers = this.accountService.makeJWTHeader();
    return this.http.get<ResponseModel<FileInfoModel[]>>(this.url + 'files/GetLastUploadedFiles/' + quantity, { headers });
  }

  getRandomFiles(quantity: number) {
    let headers = this.accountService.makeJWTHeader();
    return this.http.get<ResponseModel<FileInfoModel[]>>(this.url + 'files/GetRandomFiles/' + quantity, { headers });
  }

  deleteFile(id: number) {
    let headers = this.accountService.makeJWTHeader();
    return this.http.delete<ResponseModel<FileInfoModel>>(this.url + `files/DeleteFile/${id}`, { headers });
  }

  convertDate(date: Date): string {
    var dateToConvert: Date = new Date(date);
    var dateStr = dateToConvert.toLocaleDateString();
    var timeStr = dateToConvert.getHours() + ':' + dateToConvert.getMinutes();
    return dateStr + ' | ' + timeStr;
  }

  downloadFile(file: FileInfoModel) {
    let headers = this.accountService.makeJWTHeader();
    let accountId = this.accountService.getAccountId() as string;
    var request = new HttpRequest('GET', this.url + `files/Download/${file.id}/${accountId}`, {
      headers,
      reportProgress: true,
      responseType: 'blob'
    });
    return this.http.request<Blob>(request);
  }

  createFileLinkWithClick(fileTitle: string, file: Blob) {
    let url = URL.createObjectURL(file);
    const a = document.createElement('a');
    a.setAttribute('style', 'display:none;');
    document.body.appendChild(a);
    a.download = fileTitle;
    a.href = url;
    a.target = '_self';
    a.click();
    document.body.removeChild(a);
  }

  updateFileCategory(fileWithNewCategory: FileInfoModel) {
    let headers = this.accountService.makeJWTHeader();
    return this.http.patch<ResponseModel<FileInfoModel>>
      (this.url + 'files/UpdateCategory/' + fileWithNewCategory.id, fileWithNewCategory, { headers });
  }

  uploadFile(file: File) {
    let headers = this.accountService.makeJWTHeader();
    let accountId = this.accountService.getAccountId() as string;
    var formData = new FormData();
    formData.append('file', file);
    var request = new HttpRequest('POST', this.url + `files/UploadFile/${accountId}`, formData, {
      headers,
      reportProgress: true,

    })
    return this.http.request<ResponseModel<FileInfoModel>>(request);
  }

  getFileFields() {
    var headers = this.accountService.makeJWTHeader();
    return this.http.get<ResponseModel<string[]>>(this.url + 'files/GetFileFields', { headers });
  }

  equalizeFileServerWithDB() {
    return this.http.get<ResponseModel<FileInfoModel[]>>(this.url + 'files/equalizeFileServerWithDB');
  }

  updateDownloadStatus(model: FileInfoModel, status: boolean) {
    var headers = this.accountService.makeJWTHeader();
    return this.http.patch<ResponseModel<FileInfoModel>>(this.url + 'files/UpdateDownloadStatus/' + model.id, status, { headers });
  }
}
