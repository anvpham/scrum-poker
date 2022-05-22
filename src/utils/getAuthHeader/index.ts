import CookieReader from 'js-cookie';

const getAuthHeader = (): string | undefined => {
    const tokenExpiration = CookieReader.get('tokenExpiration');
    const jwtToken = CookieReader.get('jwtToken');
    const officialUser = CookieReader.get('officialUser');

    if(tokenExpiration && jwtToken && officialUser) {
        const expirationDate = new Date(tokenExpiration);
        const currentDate = new Date();

        if(expirationDate > currentDate) {
            return `Bearer ${CookieReader.get('jwtToken')}`;
        } else {
            const allCookies = document.cookie.split(';');
            
            for (let i = 0; i < allCookies.length; i++) {
                document.cookie = allCookies[i] + '=;expires=' + new Date(0).toUTCString();
            }
            
            return '';
        }
    } else {
        return ''
    }
}

export default getAuthHeader;
