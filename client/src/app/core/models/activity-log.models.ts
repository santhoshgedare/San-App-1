export interface ActivityLogDto {
  id: string;
  entityType: string;
  entityId: string;
  action: string;
  details: string | null;
  performedByUserId: string | null;
  performedByEmail: string | null;
  createdAt: string;
}
