export class ApiResponse<T> {
  constructor(
    public data: T,
    public success: boolean,
    public message: string
  ) {}
}
