export class AccountModel {
  constructor(Id: string, Username: string, Email: string, Phone: string,
    JWTToken: string, RegistrationDate: Date, FilesUploaded: number,
    FilesDownloaded: number, TotalSizeProcessed: number, Roles: string[]) {
    this.id = Id;
    this.phone = Phone;
    this.email = Email;
    this.filesDownloaded = FilesDownloaded;
    this.filesUploaded = FilesUploaded;
    this.username = Username;
    this.totalSizeProcessed = TotalSizeProcessed;
    this.jwtToken = JWTToken;
    this.registrationDate = RegistrationDate;
    this.roles = Roles;
  }
  public id: string;
  public username: string;
  public email: string;
  public phone: string;
  public jwtToken: string;
  public registrationDate: Date;
  public filesUploaded: number;
  public filesDownloaded: number;
  public totalSizeProcessed: number;
  public roles: string[];
} 
