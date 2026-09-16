import { MangaType, PublicationStatus } from './enums';

export interface GenreDto {
  malId: number;
  name: string;
}

export interface MangaCatalogSearchResultDto {
  malId: number;
  title: string;
  titleEnglish: string | null;
  coverImageUrl: string;
  type: MangaType;
  status: PublicationStatus | null;
}

export interface MangaCatalogSearchResponse {
  items: MangaCatalogSearchResultDto[];
  currentPage: number;
  hasNextPage: boolean;
  lastVisiblePage: number;
  totalItemsCount: number;
}

export interface MangaCatalogDetailDto extends MangaCatalogSearchResultDto {
  synopsis: string | null;
  genres: GenreDto[];
}

export interface MangaSnapshotDto extends MangaCatalogDetailDto {
  id: string;
}
