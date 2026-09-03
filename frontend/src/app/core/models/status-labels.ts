import { ReadingStatus, MangaType, PublicationStatus } from './enums';

export const statusLabels: Record<ReadingStatus, string> = {
  [ReadingStatus.Reading]: 'Leyendo',
  [ReadingStatus.Completed]: 'Completado',
  [ReadingStatus.OnHold]: 'En pausa',
  [ReadingStatus.Dropped]: 'Abandonado',
  [ReadingStatus.PlanToRead]: 'Por leer'
};

export const typeLabels: Record<MangaType, string> = {
  [MangaType.Manga]: 'Manga',
  [MangaType.Manhwa]: 'Manhwa',
  [MangaType.Manhua]: 'Manhua',
  [MangaType.LightNovel]: 'Novela ligera',
  [MangaType.OneShot]: 'One-shot',
  [MangaType.Doujinshi]: 'Doujinshi'
};

export const publicationStatusLabels: Record<PublicationStatus, string> = {
  [PublicationStatus.Publishing]: 'En publicación',
  [PublicationStatus.Finished]: 'Finalizado',
  [PublicationStatus.Hiatus]: 'En hiatus',
  [PublicationStatus.Discontinued]: 'Discontinuado'
};
