export interface TimetableOpening {
  name: string;
  openingTime: string;
  closingTime: string;
}

export interface Timetable {
  id: string;
  name: string;
  description: string | null;
  ownerId: string;
  openings: TimetableOpening[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateTimetableRequest {
  name: string;
  description?: string;
  ownerId: string;
  openings: TimetableOpening[];
}

export interface UpdateTimetableRequest {
  name: string;
  description?: string;
  openings: TimetableOpening[];
}
