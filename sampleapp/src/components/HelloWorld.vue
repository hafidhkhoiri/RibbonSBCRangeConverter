<template>
  <v-card
    class="mx-auto"
  >
    <v-row align="center">
        <v-col
          class="text-h6"
          cols="12"
          md="2"
          sm="12"
        >
          <v-avatar color="red" size="100%" height="140px" tile>
            <span class="white--text text-h5">EP</span>
          </v-avatar>
          <v-list-item two-line>
            <v-list-item-content>
              <v-list-item-title class="text-h6">
                Errai Pasifik
              </v-list-item-title>
            </v-list-item-content>
          </v-list-item>
        </v-col>
        <v-col
          class="text-h6"
          cols="10"
          md="10"
          sm="12"
        >
               <v-list-item>
                <v-list-item-content>
                  <v-list-item-title  class="text-h5 mb-1">15000</v-list-item-title>
                  <v-list-item-subtitle>Total phone numbers</v-list-item-subtitle>
                </v-list-item-content>
                <v-list-item-content>
                  <v-list-item-title  class="text-h5 mb-1">2</v-list-item-title>
                  <v-list-item-subtitle>Total Suppliers</v-list-item-subtitle>
                </v-list-item-content>
                <v-list-item-content>
                  <v-list-item-title  class="text-h5 mb-1">2021-08-09</v-list-item-title>
                  <v-list-item-subtitle>Last updated</v-list-item-subtitle>
                </v-list-item-content>
              </v-list-item>
              

      <v-card-actions>
      <v-btn
        outlined
        color="primary"
      >
        Upload numbers from CSV
      </v-btn>
    </v-card-actions>

              <v-divider></v-divider>
              <v-simple-table dense>
                <template v-slot:default>
                  <thead>
                    <tr>
                      <th class="text-left">
                        Phone Numbers
                      </th>
                      <th class="text-left">
                        Status
                      </th>
                      <th class="text-left">
                        Supplier
                      </th>
                      <th class="text-left">
                        Sync Progress
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr
                      v-for="item in numberRanges"
                      :key="item.name"
                    >
                      <td>{{ item.range }}</td>
                      <td>
                          <v-chip
                              small
                              :color="getColor(item.status)"
                              outlined
                          >
                              {{ item.status }}
                          </v-chip>
                      </td>
                      <td>{{ item.supplier }}</td>
                      <td>
                        <v-progress-linear
                          :value="item.syncProgress"
                          striped
                          color="green"
                          height="20"
                          disabled
                        >
                          <strong>{{ Math.ceil(item.syncProgress) }}%</strong>
                        </v-progress-linear>

                      </td>
                    </tr>
                  </tbody>
                </template>
              </v-simple-table>
              
        </v-col>
      </v-row>
  </v-card>
</template>

<script>
  export default {
    data () {
      return {
        labels: ['SU', 'MO', 'TU', 'WED', 'TH', 'FR', 'SA'],
        time: 0,
        forecast: [
          { day: 'Tuesday', icon: 'mdi-white-balance-sunny', temp: '24\xB0/12\xB0' },
          { day: 'Wednesday', icon: 'mdi-white-balance-sunny', temp: '22\xB0/14\xB0' },
          { day: 'Thursday', icon: 'mdi-cloud', temp: '25\xB0/15\xB0' },
        ],
        numberRanges: [
          {
            range: '628231268449 - 628231270000',
            status: "Active",
            supplier: "Telkomsel",
            syncProgress: 19
          },
          {
            range: '128231268449 - 128231270000',
            status: "Inactive",
            supplier: "VZ",
            syncProgress: 0
          },
          {
            range: '128201268449 - 128131270000',
            status: "Active",
            supplier: "Telkomsel",
            syncProgress: 55
          },
          {
            range: '428201268429 - 438131270000',
            status: "Active",
            supplier: "O2",
            syncProgress: 100
          },
        ],
      }
    },
    methods:{
      getColor(status){
        return status == 'Active' ? 'green' : 'red';
      }
    }
  }
</script>