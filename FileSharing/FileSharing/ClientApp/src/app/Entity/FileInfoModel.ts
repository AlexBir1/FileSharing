export class FileInfoModel {
  constructor(Id: number, Title: string, Path: string, Extension: string, Size: number, ContentType: string,
    CanBeDownloaded: boolean, category_Id: number, categoryTitle: string, lastDownloadDate: Date, uploadDate: Date)
  {
    this.id = Id;
    this.title = Title;
    this.path = Path;
    this.size = Size;
    this.extension = Extension;
    this.contentType = ContentType;
    this.canBeDownloaded = CanBeDownloaded;
    this.categoryTitle = categoryTitle;
    this.category_Id = category_Id;
    this.uploadDate = uploadDate;
    this.lastDownloadDate = lastDownloadDate;
  }

  public id: number;
  public title: string;
  public path: string;
  public extension: string;
  public size: number;
  public contentType: string;
  public canBeDownloaded: boolean;
  public uploadDate: Date;
  public lastDownloadDate: Date;
  public downloadCount: number = 0;
  public categoryTitle: string;
  public category_Id: number;
}
