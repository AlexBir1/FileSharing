
export class ResponseModel<T> {
  public data!: T;
  public isSuccessful: boolean = false;
  public errors: string[] = [];
}
