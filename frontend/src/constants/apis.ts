const getUrl = (path: string): string => `${HOST}/api/${path}`;
const getChannel = (path: string): string => `${HOST}/${path}`;

export const HOST = process.env.API_URL;

export const SIGN_UP = getUrl('user/signup');
export const LOGIN = getUrl('user/login');
export const AUTHENTICATE = getUrl('user/authenticate');
export const REFRESH_TOKEN = getUrl('user/refreshtoken');
export const CHANGE_NAME = getUrl('user/changename');

export const CREATE_ROOM = getUrl('room/create');
export const JOIN_ROOM = getUrl('room/join');
export const GET_ROOM_STORIES: (roomId: number) => string = (roomId: number) => getUrl(`room/${roomId}/stories`);
export const CHECK_ROOM: (roomCode: string) => string = (roomCode: string) => getUrl(`room/check/${roomCode}`);
export const ROOM_CHANNEL = getChannel('room');

export const GET_STORY = getUrl('story/get');
export const ADD_STORY = getUrl('story/add');
export const SUBMIT_POINT = getUrl('story/submitpoint');
export const DELETE_STORY = getUrl('story/delete');

export const SUBMIT_JIRA_USER_CREDENTIALS = getUrl('jira/addtoken');
export const FETCH_JIRA_STORIES = getUrl('jira/fetchstories');
export const ADD_JIRA_STORY = getUrl('jira/addstory');
export const SUBMIT_JIRA_POINT = getUrl('jira/submitpoint');
