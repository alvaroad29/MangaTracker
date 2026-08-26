export interface ApiProblemDetails {
  status: number;
  title: string;
  errors?: Record<string, string[]>; // formato ModelState del ValidationFilter
}
