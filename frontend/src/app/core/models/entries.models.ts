import { ReadingStatus } from './enums';
import { MangaSnapshotDto } from './catalog.models';

export interface MangaEntryDto {
  id: string;
  manga: MangaSnapshotDto;
  status: ReadingStatus;
  lastChapterRead: number;
  score: number | null;
  notes: string | null;
  startedAt: string | null;
  finishedAt: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface UpdateMangaEntryRequest {
  status: ReadingStatus;
  lastChapterRead: number;
  score: number | null;
  notes: string | null;
}
