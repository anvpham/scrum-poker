import { Dispatch } from 'redux';
import { SIGN_UP, LOGIN } from '@scrpoker/constants/apis';
import { ThunkAction } from 'redux-thunk';

interface IUserInfoResponse {
  jwtToken: string;
  userId: number;
  name: string;
  userRoomCode?: string;
  expiration: number;
}

export const signUp = (signUpData: ISignUpData): ThunkAction<Promise<void>, IGlobalState, unknown, IRoomAction> => (
  dispatch: Dispatch
) =>
  fetch(SIGN_UP, {
    method: 'POST',
    body: JSON.stringify(signUpData),
    headers: {
      'Content-Type': 'application/json',
    },
  })
    .then((response) => response.json())
    .then(({ jwtToken, userId, name, userRoomCode, expiration }: IUserInfoResponse) => {
      const date = new Date();
      date.setSeconds(expiration);
      document.cookie = `jwtToken=${jwtToken};expires=${date};path=/`;

      dispatch({
        type: 'UPDATE_USER_INFO',
        payload: {
          jwtToken,
          userId,
          name,
          userRoomCode,
        },
      });
    })
    .catch((err) => {
      throw new Error(err);
    });

export const login = (loginData: ILoginData): ThunkAction<Promise<void>, IGlobalState, unknown, IRoomAction> => (
  dispatch: Dispatch
) =>
  fetch(LOGIN, {
    method: 'POST',
    body: JSON.stringify(loginData),
    headers: {
      'Content-Type': 'application/json',
    },
  })
    .then((response) => response.json())
    .then(({ jwtToken, userId, name, userRoomCode, expiration }: IUserInfoResponse) => {
      const date = new Date();
      date.setSeconds(expiration);
      document.cookie = `jwtToken=${jwtToken};expires=${date};path=/`;

      dispatch({
        type: 'UPDATE_USER_INFO',
        payload: {
          jwtToken,
          userId,
          name,
          userRoomCode,
        },
      });
    })
    .catch((err) => console.log(err));

export const updateUserAction = (action: string): IUserAction => {
  return {
    type: 'UPDATE_USER_ACTION',
    payload: action,
  };
};
