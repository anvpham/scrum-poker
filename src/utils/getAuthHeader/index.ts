import CookieReader from 'js-cookie';

const getAuthHeader = (): string => {
    const tokenExpiration = CookieReader.get('tokenExpiration');
    const numberOfCookies = document.cookie.split(';');

    if(tokenExpiration && numberOfCookies.length == 3) {
        const expirationDate = new Date(tokenExpiration);
        const currentDate = new Date();

        if(expirationDate > currentDate) {
            return `Bearer ${CookieReader.get('jwtToken')}`;
        } else {
            var allCookies = document.cookie.split(';');
            
            for (var i = 0; i < allCookies.length; i++) {
                document.cookie = allCookies[i] + "=;expires=" + new Date(0).toUTCString();
            }
            
            return '';
        }
    } else {
        return ''
    }
}

export default getAuthHeader;
