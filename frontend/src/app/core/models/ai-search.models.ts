import { MangaCatalogSearchResponse } from "./catalog.models";
import { MangaType, PublicationStatus } from "./enums";

export interface MangaSearchCriteria {
  searchQuery: string;
  type: MangaType | null;
  status: PublicationStatus | null;
  genres: string[];
}

export interface AiSearchResponse {
  interpretation: MangaSearchCriteria;
  results: MangaCatalogSearchResponse;
}
